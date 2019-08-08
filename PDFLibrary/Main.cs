﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using iText.Forms;
using iText.Forms.Fields;
using iText.Kernel.Pdf;

namespace PDFLibrary
{
    public class PdfMethods

    {
        public static bool IsPDF(byte[] file)
        {

            if (file == null || file.Length == 0)
                return false;

            try
            {
                using (var ms = new MemoryStream(file))
                {
                    using (var reader = new PdfReader(ms))
                    {
                        return true;
                    }
                }
            }
            catch (iText.IO.IOException)
            {
                return false;
            }
        }


        public static string[] MissingFields(string[] fields, byte[] pdf)
        {
            return !validParameters(fields, pdf) ? null : getFormFields(pdf).Keys.ToArray().Except(fields).ToArray();
        }

        public static string[] ExtraFields(string[] fields, byte[] pdf)
        {
            return !validParameters(fields, pdf) ? null : fields.Except(getFormFields(pdf).Keys).ToArray();
        }


        public static bool? ValidateFields(string[] fields, byte[] pdf)
        {



            if (!validParameters(fields, pdf))
                return null;

            return !fields.Except(getFormFields(pdf).Keys.ToArray()).Any();
        }


        public static Dictionary<string, string> GetData(byte[] pdf)
        {
            if (!IsPDF(pdf))
                return null;


            return getFormFields(pdf).ToDictionary(x => x.Key, x => x.Value.GetValueAsString());
        }


        public static byte[] SetData(PdfField[] fields, byte[] pdf)
        {
            if (!IsPDF(pdf))
                return null;

            using (var ms = new MemoryStream())
            {
                using (var doc = new PdfDocument(new PdfReader(new MemoryStream(pdf)), new PdfWriter(ms)))

                {
                    var form = PdfAcroForm.GetAcroForm(doc, true);
                    var formFields = form.GetFormFields();
                    foreach (var field in fields.Where(x => !string.IsNullOrEmpty(x.Value) ))
                    {
                        if (string.IsNullOrEmpty(field.DisplayValue))
                        {
                            formFields[field.Name].SetValue(field.Value);
                        }
                        else
                        {
                            formFields[field.Name].SetValue(field.Value, field.DisplayValue);
                        }
                    }

                    doc.Close();
                    return ms.ToArray();
                }
            }
        }

        static IDictionary<string, PdfFormField> getFormFields(byte[] pdf)
        {


            using (var ms = new MemoryStream(pdf))
            {
                using (var reader = new PdfReader(ms))
                {
                    using (var doc = new PdfDocument(reader))
                    {
                        return PdfAcroForm.GetAcroForm(doc, false).GetFormFields();
                    }
                }
            }

        }

        

        static bool validParameters(string[] fields, byte[] pdf)
        {
            if (!IsPDF(pdf))
                return false;

            return !((fields == null || fields.Length == 0));
        }
    }




    public class PdfFields : List<PdfField>
    {
    }


    public class PdfField
    {
        public PdfField(string name, string value, string displayValue = null)
        {
            DisplayValue = displayValue;
            Value = value;
            Name = name;
        }

        public string Name { get; }
        public string Value { get; }
        public string DisplayValue { get; }
    }

}