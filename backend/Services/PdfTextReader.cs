using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;

namespace CVBuilder.API.Services
{
    /// <summary>
    /// Shared PDF-to-text extraction, used by any flow that needs to read an uploaded CV PDF.
    /// </summary>
    public static class PdfTextReader
    {
        public static string ExtractText(byte[] pdfContent)
        {
            using (var memoryStream = new MemoryStream(pdfContent))
            {
                var pdfReader = new PdfReader(memoryStream);
                var text = new System.Text.StringBuilder();

                for (int i = 1; i <= pdfReader.NumberOfPages; i++)
                {
                    // LocationTextExtractionStrategy orders text by its position on the page instead of
                    // the raw order it appears in the PDF content stream, which keeps multi-column resume
                    // layouts (photo header, sidebar skills, etc.) from coming out scrambled.
                    text.Append(PdfTextExtractor.GetTextFromPage(pdfReader, i, new LocationTextExtractionStrategy()));
                    text.Append("\n");
                }

                return text.ToString();
            }
        }
    }
}
