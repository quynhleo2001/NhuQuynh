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
    
    public class EmployeeController : Controller
    {
        //khai bao Dbcontext de lam viec voi database
        private readonly ApplicationDbContext _context;
        public EmployeeController (ApplicationDbContext context)
        {
            _context = context;
        }

        //Khai báo class ExcelProcess trong EmployeeController
        private ExcelProcess _excelProcess = new ExcelProcess();

        //Action tra ve view hien thi danh sach sinh vien
        public async Task<IActionResult> Index()
        {
            var model = await _context.Employee.ToListAsync();
            return View(model);
        }

        //Action trả về view thêm mới danh sách sinh viên
        public IActionResult Create()
        {
            return View();
        }

        //Action xử lý dữ liệu sinh viên gửi lên từ view và lưu vào database
        [HttpPost]
        public async Task<IActionResult> Create(Employee std)
        {
            if(ModelState.IsValid)
            {
                _context.Add(std);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View();
        }

        //kiem tra ma nhan vien co ton tai khong
        private bool EmployeeExists (string id)
        {
            return _context.Employee.Any(e => e.EmployeeID == id);
        }
        
        //Tạo phương thức Edit kiểm tra xem “id” của sinh viên có tồn tại trong cơ sở dữ liệu không? Nếu có thì trả về view “Edit” cho phép người dùng chỉnh sửa thông tin của Sinh viên đó.​
        // GET: Employee/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return View("NotFound");
            }

            var Employee = await _context.Employee.FindAsync(id);
            if (Employee == null)
            {
                return View("NotFound");
            }
            return View(Employee);
        }

        //Tạo phương thức Edit cập nhật thông tin của sinh viên theo mã sinh viên.
        // POST: Employee/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("EmployeeID,EmployeeName")] Employee std)
        {
            if (id != std.EmployeeID)
            {
                return View("NotFound");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(std);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EmployeeExists(std.EmployeeID))
                    {
                        return View("NotFound");
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(std);
        }

        //Tạo phương thức Delete kiểm tra xem “id” của sinh viên có tồn tại trong cơ sở dữ liệu không? Nếu có thì trả về view “Delete” cho phép người dùng xoá thông tin của Sinh viên đó.
        public async Task<IActionResult> Delete(string id)
        {
            if(id == null)
            {
                return View("NotFound");
            }

            var std = await _context.Employee.FirstOrDefaultAsync(m => m.EmployeeID == id);
            if (std == null)
            {
                return View("NotFound");
            }

            return View(std);
        }

        //Tạo phương thức Delete xoá thông tin của sinh viên theo mã sinh viên.
        //POST: Product/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var std = await _context.Employee.FindAsync(id);
            _context.Employee.Remove(std);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

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
                            //create a new Employee object
                            var emp = new Employee();
                            //set values for attribiutes
                            emp.EmployeeID = dt.Rows[i][0].ToString();
                            emp.EmployeeName = dt.Rows[i][1].ToString();
                            emp.EmployeeAdderss = dt.Rows[i][2].ToString();
                            //add oject to context
                            _context.Employee.Add(emp);
                        }
                        //save to database
                        await _context.SaveChangesAsync();
                        return RedirectToAction(nameof(Index));
                    }
                }
            }
            return View();
        }
    }
}
