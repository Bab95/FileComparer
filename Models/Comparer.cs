using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileComparer.Models
{
    public interface IComparer
    {
        void Compare(object obj);
    }
}
