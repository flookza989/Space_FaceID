using OfficeOpenXml;
using OfficeOpenXml.Style;
using Space_FaceID.Models.DTOs;
using Space_FaceID.Models.Entities;
using Space_FaceID.Models.Enums;
using Space_FaceID.Repositories.Interfaces;
using Space_FaceID.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Space_FaceID.Services.Implementation
{
    public class ExcelService(
        IUserService userService,
        IRoleService roleService,
        IUnitOfWorkRepository unitOfWork) : IExcelService
    {
        private readonly IUserService _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        private readonly IRoleService _roleService = roleService ?? throw new ArgumentNullException(nameof(roleService));
        private readonly IUnitOfWorkRepository _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));

        /// <summary>
        /// นำเข้าข้อมูลพนักงานจากไฟล์ Excel
        /// </summary>
        public async Task<ImportResult> ImportEmployeesFromExcelAsync(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("ไม่พบไฟล์ Excel ที่ระบุ", filePath);

            var result = new ImportResult
            {
                ImportStartTime = DateTime.Now
            };

            int actualRows = 0; // นับจำนวนแถวที่นำเข้าจริง

            try
            {
                // โหลดบทบาทที่มีอยู่ในระบบ
                var availableRoles = await _roleService.GetRolesWithoutAdminAsync();
                var roleDict = availableRoles.ToDictionary(r => r.Name.ToLower(), r => r);

                using var package = new ExcelPackage(new FileInfo(filePath));
                var worksheet = package.Workbook.Worksheets[0]; // สมมติว่าข้อมูลอยู่ในชีทแรก

                int rowCount = worksheet.Dimension?.Rows ?? 0;
                result.TotalRows = rowCount;



                // เริ่มจากแถวที่ 2 (ข้ามส่วนหัว)
                for (int row = 2; row <= rowCount; row++)
                {
                    try
                    {
                        // ข้ามแถวที่ว่าง
                        string username = worksheet.Cells[row, 1].GetValue<string>() ?? "";
                        if (string.IsNullOrWhiteSpace(username))
                            continue;

                        // ถ้าพบคำว่า "หมายเหตุ" หรือ "คำแนะนำ" ให้หยุดการอ่าน
                        if (username.Contains("หมายเหตุ") || username.Contains("คำแนะนำ") || username.Contains("ช่องที่") || username.Contains("หมายเหตุ:"))
                            break;

                        // เพิ่มจำนวนแถวที่นำเข้าจริง
                        actualRows++;

                        // อ่านข้อมูลจาก Excel
                        var importDto = new EmployeeImportDto
                        {
                            Username = username,
                            Password = worksheet.Cells[row, 2].GetValue<string>() ?? string.Empty,
                            Email = worksheet.Cells[row, 3].GetValue<string>(),
                            FirstName = worksheet.Cells[row, 4].GetValue<string>() ?? string.Empty,
                            LastName = worksheet.Cells[row, 5].GetValue<string>() ?? string.Empty,
                            Gender = worksheet.Cells[row, 6].GetValue<string>(),
                            PhoneNumber = worksheet.Cells[row, 7].GetValue<string>(),
                            Address = worksheet.Cells[row, 8].GetValue<string>(),
                            Role = worksheet.Cells[row, 9].GetValue<string>() ?? "User"
                        };

                        // แปลงค่าวันเกิด
                        var dateOfBirthCell = worksheet.Cells[row, 10].Value;
                        if (dateOfBirthCell != null)
                        {
                            if (DateTime.TryParse(dateOfBirthCell.ToString(), out DateTime dob))
                            {
                                importDto.DateOfBirth = dob;
                            }
                        }

                        // แปลงค่าสถานะการใช้งาน
                        var isActiveCell = worksheet.Cells[row, 11].Value;
                        if (isActiveCell != null)
                        {
                            string isActiveStr = isActiveCell.ToString()?.ToLower() ?? "true";
                            if (isActiveStr == "true" || isActiveStr == "yes" || isActiveStr == "y" || isActiveStr == "1")
                            {
                                importDto.IsActive = true;
                            }
                            else
                            {
                                importDto.IsActive = false;
                            }
                        }

                        // ตรวจสอบความถูกต้องของข้อมูล
                        var validationResults = new List<ValidationResult>();
                        var validationContext = new ValidationContext(importDto);
                        if (!Validator.TryValidateObject(importDto, validationContext, validationResults, true))
                        {
                            // มีข้อผิดพลาดในการตรวจสอบความถูกต้อง
                            string errorMessage = string.Join(", ", validationResults.Select(r => r.ErrorMessage));
                            result.Errors.Add(new ImportError
                            {
                                RowNumber = row,
                                ErrorMessage = errorMessage,
                                Username = importDto.Username
                            });
                            continue;
                        }

                        // ตรวจสอบว่ามีชื่อผู้ใช้ซ้ำหรือไม่
                        var existingUser = await _userService.GetUserByUsernameAsync(importDto.Username);
                        if (existingUser != null)
                        {
                            result.Errors.Add(new ImportError
                            {
                                RowNumber = row,
                                ErrorMessage = "ชื่อผู้ใช้นี้มีอยู่ในระบบแล้ว",
                                Username = importDto.Username
                            });
                            continue;
                        }

                        // ตรวจสอบบทบาท
                        Role userRole;
                        string roleName = importDto.Role.ToLower();
                        if (roleDict.TryGetValue(roleName, out var role))
                        {
                            userRole = role;
                        }
                        else
                        {
                            // ถ้าไม่พบบทบาทที่ระบุ ใช้บทบาท User เป็นค่าเริ่มต้น
                            userRole = roleDict.TryGetValue("user", out Role? value) ? value : availableRoles.First();
                        }

                        // สร้างข้อมูลผู้ใช้ใหม่
                        var newUser = new User
                        {
                            Username = importDto.Username,
                            Email = importDto.Email,
                            IsActive = importDto.IsActive,
                            RoleId = userRole.Id,
                            Profile = new UserProfile
                            {
                                FirstName = importDto.FirstName ?? "",
                                LastName = importDto.LastName ?? "",
                                PhoneNumber = importDto.PhoneNumber,
                                Gender = importDto.Gender,
                                DateOfBirth = importDto.DateOfBirth,
                                Address = importDto.Address
                            }
                        };

                        // ลงทะเบียนผู้ใช้ใหม่
                        var registeredUser = await _userService.RegisterAsync(newUser, importDto.Password);
                        result.SuccessfulImports.Add(registeredUser);
                    }
                    catch (Exception ex)
                    {
                        // จับข้อผิดพลาดที่เกิดขึ้นในแต่ละแถว
                        result.Errors.Add(new ImportError
                        {
                            RowNumber = row,
                            ErrorMessage = $"เกิดข้อผิดพลาด: {ex.Message}"
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                // จับข้อผิดพลาดทั่วไป
                throw new Exception($"เกิดข้อผิดพลาดในการนำเข้าข้อมูล: {ex.Message}", ex);
            }
            finally
            {
                result.ImportEndTime = DateTime.Now;
                result.ActualRows = actualRows; // กำหนดจำนวนแถวที่นำเข้าจริง
            }

            return result;
        }

        /// <summary>
        /// สร้างเทมเพลต Excel สำหรับนำเข้าข้อมูลพนักงาน
        /// </summary>
        public async Task<bool> CreateEmployeeTemplateAsync(string filePath)
        {
            return await Task.Run(() =>
             {
                 try
                 {
                     // สร้างโฟลเดอร์หากยังไม่มี
                     var directory = Path.GetDirectoryName(filePath);
                     if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                     {
                         Directory.CreateDirectory(directory);
                     }

                     using var package = new ExcelPackage();

                     // เพิ่มชีทใหม่
                     var worksheet = package.Workbook.Worksheets.Add("Employees");

                     // เปลี่ยนตระกูลฟอนต์และขนาดฟอนต์สำหรับทั้ง worksheet
                     worksheet.Cells.Style.Font.Name = "Angsana New";
                     worksheet.Cells.Style.Font.Size = 16;

                     // ตั้งค่าหัวตาราง
                     string[] headers =
                     [
                    "Username*",
                    "Password*",
                    "Email",
                    "FirstName",
                    "LastName",
                    "Gender",
                    "PhoneNumber",
                    "Address",
                    "Role*",
                    "DateOfBirth",
                    "IsActive"
                     ];

                     for (int i = 0; i < headers.Length; i++)
                     {
                         worksheet.Cells[1, i + 1].Value = headers[i];
                     }

                     // จัดรูปแบบหัวตาราง
                     using (var range = worksheet.Cells[1, 1, 1, headers.Length])
                     {
                         range.Style.Font.Bold = true;
                         range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                         range.Style.Fill.BackgroundColor.SetColor(Color.LightBlue);
                         range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                     }

                     // เพิ่มข้อมูลตัวอย่าง
                     worksheet.Cells[2, 1].Value = "employee1";
                     worksheet.Cells[2, 2].Value = "Password123";
                     worksheet.Cells[2, 3].Value = "employee1@example.com";
                     worksheet.Cells[2, 4].Value = "สมชาย";
                     worksheet.Cells[2, 5].Value = "มีสุข";
                     worksheet.Cells[2, 6].Value = "ชาย";
                     worksheet.Cells[2, 7].Value = "0812345678";
                     worksheet.Cells[2, 8].Value = "123 หมู่ 4 ต.หนองหาร อ.สันทราย จ.เชียงใหม่ 50290";
                     worksheet.Cells[2, 9].Value = "User";
                     worksheet.Cells[2, 10].Value = new DateTime(1990, 1, 1);
                     worksheet.Cells[2, 11].Value = true;

                     // กำหนดรูปแบบการแสดงผลสำหรับคอลัมน์ต่างๆ
                     worksheet.Column(1).Style.Numberformat.Format = "@"; // Username เป็นข้อความ
                     worksheet.Column(2).Style.Numberformat.Format = "@"; // Password เป็นข้อความ
                     worksheet.Column(3).Style.Numberformat.Format = "@"; // Email เป็นข้อความ
                     worksheet.Column(4).Style.Numberformat.Format = "@"; // FirstName เป็นข้อความ
                     worksheet.Column(5).Style.Numberformat.Format = "@"; // LastName เป็นข้อความ
                     worksheet.Column(6).Style.Numberformat.Format = "@"; // Gender เป็นข้อความ
                     worksheet.Column(7).Style.Numberformat.Format = "@"; // PhoneNumber เป็นข้อความ
                     worksheet.Column(8).Style.Numberformat.Format = "@"; // Address เป็นข้อความ
                     worksheet.Column(9).Style.Numberformat.Format = "@"; // Role เป็นข้อความ
                     worksheet.Column(10).Style.Numberformat.Format = "dd/MM/yyyy"; // DateOfBirth เป็นวันที่
                     worksheet.Column(11).Style.Numberformat.Format = "@"; // IsActive เป็นข้อความ (TRUE/FALSE)

                     // เพิ่มการตรวจสอบความถูกต้องของข้อมูล (Data Validation)

                     // 1. Gender - กำหนดรายการแบบเลือกได้ (Drop-down list)
                     var genderValidation = worksheet.DataValidations.AddListValidation("F2:F1000");
                     genderValidation.Formula.Values.Add("ชาย");
                     genderValidation.Formula.Values.Add("หญิง");
                     genderValidation.Formula.Values.Add("อื่นๆ");
                     genderValidation.ShowErrorMessage = true;
                     genderValidation.ErrorTitle = "เพศไม่ถูกต้อง";
                     genderValidation.Error = "กรุณาเลือกเพศจากรายการที่กำหนดเท่านั้น";

                     // 2. Role - กำหนดรายการแบบเลือกได้ (Drop-down list)
                     var roleValidation = worksheet.DataValidations.AddListValidation("I2:I1000");
                     roleValidation.Formula.Values.Add("User");
                     roleValidation.Formula.Values.Add("Supervisor");
                     roleValidation.ShowErrorMessage = true;
                     roleValidation.ErrorTitle = "บทบาทไม่ถูกต้อง";
                     roleValidation.Error = "กรุณาเลือกบทบาทจากรายการที่กำหนดเท่านั้น";

                     // 3. IsActive - กำหนดให้เลือกได้เฉพาะ TRUE หรือ FALSE
                     var isActiveValidation = worksheet.DataValidations.AddListValidation("K2:K1000");
                     isActiveValidation.Formula.Values.Add("TRUE");
                     isActiveValidation.Formula.Values.Add("FALSE");
                     isActiveValidation.ShowErrorMessage = true;
                     isActiveValidation.ErrorTitle = "ค่าไม่ถูกต้อง";
                     isActiveValidation.Error = "กรุณาเลือก TRUE หรือ FALSE เท่านั้น";

                     // 4. DateOfBirth - กำหนดช่วงวันที่ให้น้อยกว่าวันปัจจุบัน
                     var dateValidation = worksheet.DataValidations.AddDateTimeValidation("J2:J1000");
                     dateValidation.Operator = OfficeOpenXml.DataValidation.ExcelDataValidationOperator.lessThan;
                     dateValidation.Formula.Value = DateTime.Today;
                     dateValidation.ShowErrorMessage = true;
                     dateValidation.ErrorTitle = "วันเกิดไม่ถูกต้อง";
                     dateValidation.Error = "วันเกิดต้องน้อยกว่าวันปัจจุบัน";

                     // เพิ่มคำแนะนำและหมายเหตุ
                     worksheet.Cells[4, 1].Value = "หมายเหตุ:";
                     worksheet.Cells[4, 1].Style.Font.Bold = true;

                     string[] notes =
                     [
                    "ช่องที่มี * จำเป็นต้องระบุ",
                    "Username: ต้องมีความยาวอย่างน้อย 3 ตัวอักษร และไม่ซ้ำกับที่มีอยู่ในระบบ",
                    "Password: ต้องมีความยาวอย่างน้อย 6 ตัวอักษร",
                    "Gender: ระบุเพศ เช่น 'ชาย', 'หญิง', 'อื่นๆ'",
                    "Role: ระบุบทบาท เช่น 'User', 'Supervisor' (ค่าเริ่มต้นคือ 'User')",
                    "DateOfBirth: ระบุในรูปแบบ วัน/เดือน/ปี เช่น 01/01/1990 และต้องน้อยกว่าวันปัจจุบัน",
                    "IsActive: ระบุ TRUE หรือ FALSE (ค่าเริ่มต้นคือ TRUE)",
                    "FirstName และ LastName: สามารถเว้นว่างได้"
                     ];

                     for (int i = 0; i < notes.Length; i++)
                     {
                         worksheet.Cells[5 + i, 1].Value = notes[i];
                         worksheet.Cells[5 + i, 1, 5 + i, 5].Merge = true;
                     }

                     // ปรับความกว้างของคอลัมน์ให้พอดีกับเนื้อหา
                     worksheet.Cells.AutoFitColumns();

                     // บันทึกไฟล์
                     var fileInfo = new FileInfo(filePath);
                     package.SaveAs(fileInfo);

                     return true;
                 }
                 catch (Exception ex)
                 {
                     throw new Exception($"เกิดข้อผิดพลาดในการสร้างเทมเพลต Excel: {ex.Message}", ex);
                 }
             });
        }
    }
}
