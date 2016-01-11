using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TarikGegic.Models
{
    public class Picture
    {
        public IEnumerable<HttpPostedFileBase> Files { get; set; }
      /*  public String ImagePath
        {
            get
            {
                return "~/Content/Image/";
            }
        }*/
    }    
}