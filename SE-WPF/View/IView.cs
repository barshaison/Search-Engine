using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SE_WPF.View
{
    public delegate void viewEventDelegate(string command);
    /// <summary>
    /// iterface of the view layer
    /// </summary>
    interface IView
    {
        event viewEventDelegate ViewChanged;

        void Start();
        void Output(string output);
        string getIndexParameters();
        //string getPathToDict();
        void outputDic(Dictionary<string, long> dictionary);
        string getPathToSaveDic();
        //void outputLang(HashSet<string> langs);
        string getDictPath(ref bool isLoadSelected);
        string getSingleQuery(ref bool searchPressed);
        void outputSearchresult(List<string> searchResults);
        void outputFileSearchResult(Dictionary<string, List<string>> searchResults);
        List<string> getChosenLangs(HashSet<string> lang);
        string getQuriesFilePath(ref bool searchPressed);
        string getSaveQueryParams(ref bool savePressed);
    }
}
