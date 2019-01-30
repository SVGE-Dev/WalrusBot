using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WalrusBot
{
    /*
    class SheetsDataStore
    {
        private string sheetId;
        private string range;
        private ushort timeout = 3;

        public SheetsDataStore(string sSheetId, string sRange)
        {
            sheetId = sSheetId;
            range = sRange;
        }

        public bool Push(ref SheetsService sheetsService)
        {

        }
        public bool Pull(ref SheetsService sheetsService, bool print)
        {
            ushort attempts = 0;
            do
            {
                try
                {
                    SpreadsheetsResource.ValuesResource.GetRequest request = sheetsService.Spreadsheets.Values.Get(sheetId, range);
                    ValueRange response = request.Execute();
                    IList<IList<Object>> values = response.Values;
                    return true;
                }
                catch
                {
                    attempts++;
                }
                
            } while (attempts < timeout);
            return false;
        }
    }*/
}
