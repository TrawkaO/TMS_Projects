﻿using DALInterfaces.Models.Books;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DALInterfaces.DataModels.Books
{
    public class AuthorDataModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int YearOfIssue { get; set; }
        public virtual ICollection<Author> Authors { get; set; }
    }
}
