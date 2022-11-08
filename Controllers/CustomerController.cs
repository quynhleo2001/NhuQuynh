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
    
    public class CustomerController : Controller
    {
        //khai bao Dbcontext de lam viec voi database
        private readonly ApplicationDbContext _context;
        public CustomerController (ApplicationDbContext context)
        {
            _context = context;
        }

        //Action tra ve view hien thi danh sach sinh vien
        public async Task<IActionResult> Index()
        {
            var model = await _context.Customer.ToListAsync();
            return View(model);
        }

        //Action trả về view thêm mới danh sách sinh viên
        public IActionResult Create()
        {
            return View();
        }

        //Action xử lý dữ liệu sinh viên gửi lên từ view và lưu vào database
        [HttpPost]
        public async Task<IActionResult> Create(Customer std)
        {
            if(ModelState.IsValid)
            {
                _context.Add(std);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View();
        }

        //kiem tra ma sinh vien co ton tai khong
        private bool CustomerExists (string id)
        {
            return _context.Customer.Any(e => e.CustomerID == id);
        }
        
        //Tạo phương thức Edit kiểm tra xem “id” của sinh viên có tồn tại trong cơ sở dữ liệu không? Nếu có thì trả về view “Edit” cho phép người dùng chỉnh sửa thông tin của Sinh viên đó.​
        // GET: Customer/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return View("NotFound");
            }

            var Customer = await _context.Customer.FindAsync(id);
            if (Customer == null)
            {
                return View("NotFound");
            }
            return View(Customer);
        }

        //Tạo phương thức Edit cập nhật thông tin của sinh viên theo mã sinh viên.
        // POST: Customer/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("CustomerID,CustomerName")] Customer std)
        {
            if (id != std.CustomerID)
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
                    if (!CustomerExists(std.CustomerID))
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

            var std = await _context.Customer.FirstOrDefaultAsync(m => m.CustomerID == id);
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
            var std = await _context.Customer.FindAsync(id);
            _context.Customer.Remove(std);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private ExcelProcess _excelProcess = new ExcelProcess();
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
                            //create a new Customer object
                            var cus = new Customer();
                            //set values for attribiutes
                            cus.CustomerID = dt.Rows[i][0].ToString();
                            cus.CustomerName = dt.Rows[i][1].ToString();
                            cus.CustomerAddress = dt.Rows[i][2].ToString();
                            //add oject to context
                            _context.Customer.Add(cus);
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
    