using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NhuQuynh.Models;
using NhuQuynh.Models.Process;

namespace NhuQuynh.Controllers
{

    public class StudentController : Controller
    {
        //khai bao Dbcontext de lam viec voi database
        private readonly ApplicationDbContext _context;
        public StudentController (ApplicationDbContext context)
        {
            _context = context;
        }

        //Action tra ve view hien thi danh sach sinh vien
        public async Task<IActionResult> Index()
        {
            var model = await _context.Student.ToListAsync();
            return View(model);
        }

        // //Action trả về view thêm mới danh sách sinh viên
        // public IActionResult Create()
        // {
        //     return View();
        // }

        // //Action xử lý dữ liệu sinh viên gửi lên từ view và lưu vào database
        // [HttpPost]
        // public async Task<IActionResult> Create(Student std)
        // {
        //     if(ModelState.IsValid)
        //     {
        //         _context.Add(std);
        //         await _context.SaveChangesAsync();
        //         return RedirectToAction(nameof(Index));
        //     }
        //     return View();
        // }

        //kiem tra ma sinh vien co ton tai khong
        private bool StudentExists (string id)
        {
            return _context.Student.Any(e => e.StudentID == id);
        }
        
        // //Tạo phương thức Edit kiểm tra xem “id” của sinh viên có tồn tại trong cơ sở dữ liệu không? Nếu có thì trả về view “Edit” cho phép người dùng chỉnh sửa thông tin của Sinh viên đó.​
        // // GET: Student/Edit/5
        // public async Task<IActionResult> Edit(string id)
        // {
        //     if (id == null)
        //     {
        //         return View("NotFound");
        //     }

        //     var student = await _context.Students.FindAsync(id);
        //     if (student == null)
        //     {
        //         return View("NotFound");
        //     }
        //     return View(student);
        // }

        // //Tạo phương thức Edit cập nhật thông tin của sinh viên theo mã sinh viên.
        // // POST: Student/Edit/5
        // [HttpPost]
        // [ValidateAntiForgeryToken]
        // public async Task<IActionResult> Edit(string id, [Bind("StudentID,StudentName")] Student std)
        // {
        //     if (id != std.StudentID)
        //     {
        //         return View("NotFound");
        //     }

        //     if (ModelState.IsValid)
        //     {
        //         try
        //         {
        //             _context.Update(std);
        //             await _context.SaveChangesAsync();
        //         }
        //         catch (DbUpdateConcurrencyException)
        //         {
        //             if (!StudentExists(std.StudentID))
        //             {
        //                 return View("NotFound");
        //             }
        //             else
        //             {
        //                 throw;
        //             }
        //         }
        //         return RedirectToAction(nameof(Index));
        //     }
        //     return View(std);
        // }

        // //Tạo phương thức Delete kiểm tra xem “id” của sinh viên có tồn tại trong cơ sở dữ liệu không? Nếu có thì trả về view “Delete” cho phép người dùng xoá thông tin của Sinh viên đó.
        // public async Task<IActionResult> Delete(string id)
        // {
        //     if(id == null)
        //     {
        //         return View("NotFound");
        //     }

        //     var std = await _context.Students.FirstOrDefaultAsync(m => m.StudentID == id);
        //     if (std == null)
        //     {
        //         return View("NotFound");
        //     }

        //     return View(std);
        // }

        // //Tạo phương thức Delete xoá thông tin của sinh viên theo mã sinh viên.
        // //POST: Product/Delete/5
        // [HttpPost, ActionName("Delete")]
        // [ValidateAntiForgeryToken]
        // public async Task<IActionResult> DeleteConfirmed(string id)
        // {
        //     var std = await _context.Students.FindAsync(id);
        //     _context.Students.Remove(std);
        //     await _context.SaveChangesAsync();
        //     return RedirectToAction(nameof(Index));
        // }

        //Khai báo class ExcelProcess trong StudentController
        private ExcelProcess _excelProcess = new ExcelProcess();

        public async Task<IActionResult> Upload()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult>Upload(IFormFile file)
        {
            if (file!=null)
            {
                string fileExtension = Path.GetExtension(file.FileName);
                if (fileExtension != ".xls" && fileExtension != ".xlsx")
                {
                    ModelState.AddModelError("", "Please choose excel file to upload!");
                }
                else
                {
                    //rename file when upload to sever
                    var fileName = DateTime.Now.ToShortTimeString() + fileExtension;
                    var filePath = Path.Combine(Directory.GetCurrentDirectory() + "/Uploads/Excels", fileName);
                    var fileLocation = new FileInfo(filePath).ToString();
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        //save file to server
                        await file.CopyToAsync(stream);
                        //read data from file and write to database
                        var dt = _excelProcess.ExcelToDataTable(fileLocation);
                        //using for loop to read data form dt
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            //create a new Student object
                            var std = new Student();
                            //set values for attribiutes
                            std.StudentID = dt.Rows[i][0].ToString();
                            std.StudentName = dt.Rows[i][1].ToString();
                            std.StudentAddress = dt.Rows[i][2].ToString();
                            //add oject to context
                            _context.Student.Add(std);
                        }
                        //save to database
                        await _context.SaveChangesAsync();
                        return RedirectToAction(nameof(Index));
                    }
                }
            }
            return View();
    }

        public override bool Equals(object? obj)
        {
            return obj is StudentController controller &&
                   EqualityComparer<ExcelProcess>.Default.Equals(_excelProcess, controller._excelProcess);
        }
    }
}
   