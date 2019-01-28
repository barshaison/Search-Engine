using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SE_WPF.Model
{
    /// <summary>
    /// model layer delegate
    /// </summary>
    /// <param name="problemCode"></param>
    public delegate void modelEventDelegate(string problemCode);
    /// <summary>
    /// model layer interface
    /// </summary>
    interface IModel
    {
        /// <summary>
        /// event handler
        /// </summary>
        event modelEventDelegate ModelChanged;
        /// <summary>
        /// Generates index
        /// </summary>
        /// <param name="pathToCorpus">path to corpus</param>
        /// <param name="pathToStopWords">path to stop words</param>
        /// <param name="isStemOn">stem option</param>
        /// <param name="savePosOn">path to save</param>
        void GenerateIndex(string pathToCorpus, string pathToStopWords, bool isStemOn,string savePosOn);
        /// <summary>
        /// gets the dictionary
        /// </summary>
        /// <returns></returns>
        Dictionary<string/*term*/, long/*ptr*/> GetDictionary();
        /// <summary>
        /// save dictionary on disk
        /// </summary>
        /// <param name="path">path</param>
        void saveDicOnDisk(string path);
        /// <summary>
        /// resets the program
        /// </summary>
        /// <param name="pathToIndOnDisk">path to index</param>
        void reset(string pathToIndOnDisk);
        /// <summary>
        /// gets the languages
        /// </summary>
        /// <returns>set of languages</returns>
        HashSet<string> getLangs();
        /// <summary>
        /// gets the dictionary
        /// </summary>
        /// <returns></returns>
        Dictionary<string, long> GetRealDictionary();
        /// <summary>
        /// laod dictionary
        /// </summary>
        /// <param name="pathToDict"></param>
        /// <param name="pathToDocsLength"></param>
        /// <param name="nPath"></param>
        /// <param name="postingPath"></param>
        /// <param name="docLangDict"></param>
        /// <param name="langs"></param>
        /// <param name="dicTFpath"></param>
        void LoadDictionary(string pathToDict, string pathToDocsLength, string nPath, string postingPath, string docLangDict, string langs, string dicTFpath);
        /// <summary>
        /// perform singlequery search
        /// </summary>
        /// <param name="query"></param>
        /// <param name="chosenLangs"></param>
        void performSearch(string query, List<string/*lang*/> chosenLangs);
        /// <summary>
        /// get search results
        /// </summary>
        /// <returns></returns>
        List<string> getSearchResults();
        /// <summary>
        /// perform file query search
        /// </summary>
        /// <param name="pathToFileQ"></param>
        /// <param name="chosenLangs"></param>
        void performFileSearch(string pathToFileQ, List<string/*lang*/> chosenLangs);
        /// <summary>
        /// get file search results
        /// </summary>
        /// <returns></returns>
        Dictionary<string/*query id*/, List<string>/*doc*/> getFileSearchRes();
        /// <summary>
        /// saves query results on disk
        /// </summary>
        /// <param name="path"></param>
        /// <param name="what"></param>
        void saveQueryResults(string path, string what);
    }

    
}
