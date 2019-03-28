using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProductionTracker.Web.Models
{
    public class BarcodeLable
    {
         public string Barcode       {get;set;}
         public int    Id            {get;set;}
         public string SKU           {get;set;}
         public string Departmet     {get;set;}
         public string BodyStyle     {get;set;}
         public string BodyStyleAtt  {get;set;}
         public string Sleeve        {get;set;}
         public int    ColorId       {get;set;}
         public string Color         {get;set;}
         public string Material      {get;set;}
         public string Size          {get;set;}
         public string SizeDis       { get; set; }
         public int MaterialId { get; set; }
    }
    public class TestLable
    {
        public string Barcode { get; set; }
        public int Id { get; set; }
        public string SKU { get; set; }
        public string Sleeve { get; set; }
    }
}