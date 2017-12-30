# DNetORM4.0

Instructions

http://www.cnblogs.com/DNetORM/p/8000373.html

1.add
            using (DNetContext db = new DNetContext())
            {
                var authorid = db.Add(new Author { AuthorName = "jim", Age = 30, IsValid = true });
                db.Add(new Book { BookName = "c#", Price = 20.5, PublishDate = DateTime.Now, AuthorID = authorid });
            }
            
            

if you have some advice please send email to 307474178@qq.com
