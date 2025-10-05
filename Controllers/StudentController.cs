using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SchoolManagementAPI.Models;

namespace SchoolManagementAPI.Controllers
{
    [Route("api/student")]
    [ApiController]
    public class StudentController : ControllerBase
    {
        private readonly ApplicationDbContext _dbcontext;

        public StudentController(ApplicationDbContext dbcontext)
        {
            _dbcontext = dbcontext;
        }

        [HttpGet]
        public IActionResult GetStudents()
        {
            var students = _dbcontext.Students.ToList();

            if(students == null || !students.Any())
            {
                return NotFound(new {message = "No Students Found."});
            }

            foreach(var student in students)
            {
                if (!string.IsNullOrEmpty(student.ImageUrl) && !student.ImageUrl.StartsWith("http"))
                {
                    var fullImageUrl = $"{Request.Scheme}://{Request.Host}{student.ImageUrl}";
                    student.ImageUrl = fullImageUrl; // Update to full URL for response
                }
            }

            return Ok(students);
        }

        [HttpGet("id/{id}")]
        public IActionResult GetStudentById(int id)
        {
            var student = _dbcontext.Students.FirstOrDefault(s => s.Id == id);
            if(student == null)
            {
                return NotFound(new {message = "Student ID Not Found.", id = id});
            }
            if (!string.IsNullOrEmpty(student.ImageUrl) && !student.ImageUrl.StartsWith("http"))
            {
                var fullImageUrl = $"{Request.Scheme}://{Request.Host}{student.ImageUrl}";
                student.ImageUrl = fullImageUrl; // Update to full URL for response
            }
            return Ok(student); 
        }

        [HttpGet("name/{name}")]
        public IActionResult GetStudentByName(string name)
        {
            var student = _dbcontext.Students.FirstOrDefault(s => s.FirstName.ToLower() == name.ToLower() || s.LastName.ToLower() == name.ToLower());
            if (student == null)
            {
                return NotFound(new { message = "Student Name Not Found", name = name });
            }
            if(!string.IsNullOrEmpty(student.ImageUrl) && !student.ImageUrl.StartsWith("http"))
            {
                var fullImageUrl = $"{Request.Scheme}://{Request.Host}{student.ImageUrl}";
                student.ImageUrl = fullImageUrl; // Update to full URL for response
            }
            return Ok(student);
        }

        [HttpPost]
        public async Task<IActionResult> PostStudent([FromForm] PostStudent student)
        {
            var file = student.Image;
            if(file == null || file.Length == 0)
            {
                return BadRequest(new {message = "Image File Is Required."});
            }

            var allowedExtensions = new[] { "image/jpg", "image/jpeg", "image/png"};
            if (!allowedExtensions.Contains(file.ContentType)){
                return BadRequest(new {message = "Only JPG, JPEG, PNG files are allowed."});
            }

            var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
            if(!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
            }

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}"; // Generate a unique file name
            var filePath = Path.Combine(uploadPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var newStudent = new Student
            {
                FirstName = student.FirstName,
                LastName = student.LastName,
                DateOfBirth = student.DateOfBirth,
                Gender = student.Gender,
                Email = student.Email,
                PhoneNumber = student.PhoneNumber,
                Address = student.Address,
                ImageUrl = $"/images/{fileName}", // Store relative path to the image
                ParentName = student.ParentName,
                ParentContact = student.ParentContact
            };

            _dbcontext.Students.Add(newStudent);
            await _dbcontext.SaveChangesAsync();

            var fullImageUrl = $"{Request.Scheme}://{Request.Host}{newStudent.ImageUrl}";
            newStudent.ImageUrl = fullImageUrl; // Update to full URL for response

            return CreatedAtAction(nameof(GetStudents), new { student = newStudent });
        }

        [HttpPut]
        public async Task<IActionResult> PutStudent([FromForm] PutStudent student)
        {
            var existingStudent = await _dbcontext.Students.FindAsync(student.Id);
            if(existingStudent == null)
            {
                return NotFound(new {message = "Student ID Not Found.", id = student.Id});
            }

            existingStudent.FirstName = student.FirstName;
            existingStudent.LastName = student.LastName;
            existingStudent.DateOfBirth = student.DateOfBirth;
            existingStudent.Gender = student.Gender;
            existingStudent.Email = student.Email;
            existingStudent.PhoneNumber = student.PhoneNumber;
            existingStudent.Address = student.Address;
            existingStudent.ParentName = student.ParentName;
            existingStudent.ParentContact = student.ParentContact;

            if(student.Image != null && student.Image.Length > 0)
            {
                var file = student.Image;
                var allowedExtensions = new[] { "image/jpg", "image/jpeg", "image/png"};
                if (!allowedExtensions.Contains(file.ContentType)){
                    return BadRequest(new {message = "Only JPG, JPEG, PNG files are allowed."});
                }
                var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
                if(!Directory.Exists(uploadPath))
                {
                    Directory.CreateDirectory(uploadPath);
                }
                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}"; // Generate a unique file name
                var filePath = Path.Combine(uploadPath, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
                
                if(!string.IsNullOrEmpty(existingStudent.ImageUrl))
                {
                    var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", existingStudent.ImageUrl.TrimStart('/'));
                    if(System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath); // Delete old image file
                    }
                }
                    
                existingStudent.ImageUrl = $"/images/{fileName}"; // Update to new image path                
            }

            _dbcontext.Students.Update(existingStudent);
            await _dbcontext.SaveChangesAsync();

            var fullImageUrl = $"{Request.Scheme}://{Request.Host}{existingStudent.ImageUrl}";
            existingStudent.ImageUrl = fullImageUrl; // Update to full URL for response

            return Accepted(new {message = "Student Updated Successfully.", student = existingStudent});
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteStudent(int id)
        {
            var student = _dbcontext.Students.Find(id);
            if (student == null)
            {   
                return NotFound(new { message = "Student ID Not Found.", id = id });
            }
            if (!string.IsNullOrEmpty(student.ImageUrl))
            {
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", student.ImageUrl.TrimStart('/'));
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath); // Delete the image file
                }   
            }
            _dbcontext.Students.Remove(student);
            _dbcontext.SaveChanges();
            return  Accepted    (new { message = "Student Deleted Successfully.", id = id });
        }
    }
}
