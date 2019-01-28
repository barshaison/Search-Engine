using MathNet.Numerics.LinearAlgebra.Double;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

namespace SE_WPF.Model
{
    /// <summary>
    /// holds query word's data
    /// </summary>
    public struct queryWordData
    {
        public int r_i;
        public int n_i;
        public int qf_i;
        public Dictionary<string/*doc ID*/, int/*f_i*/> f_i;
    }
    /// <summary>
    /// This class represents a searcher, that search for relevant documents for a given query
    /// </summary>
    class Searcher
    {
        private Dictionary<string/*doc ID*/, double /*rank*/> m_docsRanks; //set of docs to chose the 50 from
        //private string m_query;
        private Dictionary<string/*word in query*/, queryWordData> m_queryWordsData;
        private HashSet<string/*doc Id*/> m_docsList; //set of docs to chose the 50 from
        private int m_N; //num of docs in the corpus
        private double m_k1 = 1.2;
        private double m_k2 = 500; // varies from 0 to 1000
        private double m_b = 0.75;
        private double m_avdl;
        //private string m_pathTo_qrels;
        private string m_pathToPostingFile;
        private Dictionary<string/*word*/, string/*posting rec*/> m_postingRecOfWord;
        private Dictionary<string/*term*/, long/*ptr to pos rec*/> m_Dictionary;
        private string[] m_queryWords;
        private Dictionary<string/*docID*/, int/*doc length*/> m_docsLength;
        private List<string/*synonyms*/> m_synonymes;
        private MathNet.Numerics.LinearAlgebra.Matrix<double> m_Tk;
        private MathNet.Numerics.LinearAlgebra.Matrix<double> m_Sk_invers;
        private MathNet.Numerics.LinearAlgebra.Matrix<double> m_Dk_T;
        private MathNet.Numerics.LinearAlgebra.Vector<double> m_q;
        private Dictionary<string/*word*/, int/*row*/> m_wordsMap;
        private Dictionary<string/*word*/, int/*col*/> m_docsMap;
        HashSet<string> m_stopWords;

        /// <summary>
        /// c'tor
        /// </summary>
        public Searcher()
        {

        }
        /// <summary>
        /// sets relevant data for the searcher
        /// </summary>
        /// <param name="N"></param>
        /// <param name="Dictionary"></param>
        /// <param name="pathToPostingFile"></param>
        /// <param name="docsLength"></param>
        /// <param name="avdl"></param>
        /// <param name="pathToStopWords"></param>
        public void setSearcher(int N,/*string pathTo_qrels,*/ Dictionary<string/*term*/, long/*ptr to pos rec*/> Dictionary, string pathToPostingFile, Dictionary<string/*docID*/, int/*doc length*/> docsLength, int avdl, string pathToStopWords)
        {
            m_avdl = avdl;
            m_N = N;
            //m_pathTo_qrels = pathTo_qrels;
            m_Dictionary = Dictionary; // [term , ptr to psting rec]
            m_pathToPostingFile = pathToPostingFile;
            m_docsLength = docsLength;
            makeStopWordsList(pathToStopWords);
        }

        //make Stop Words List
        private void makeStopWordsList(string path)
        {
            StreamReader file = new StreamReader(path);
            m_stopWords = new HashSet<string>();
            string line;
            while ((line = file.ReadLine()) != null)
            {
                line = line.ToUpper();
                m_stopWords.Add(line);
            }
        }
        /// <summary>
        /// main function that gives result for given query
        /// </summary>
        /// <param name="query"></param>
        /// <param name="docsLangDict"></param>
        /// <param name="langsChosen"></param>
        /// <returns></returns>
        public List<string/*doc ID*/> ansForQuery(string query, Dictionary<string/*doc*/, string/*lang*/> docsLangDict, List<string/*language*/> langsChosen)
        {
            List<string> docsForQuery;
            m_synonymes = new List<string>();
            List<string> queryClean = cleanFromStopWords(query);
            m_queryWords = queryClean.ToArray();
            foreach (string word in m_queryWords)
            {
                List<string> syns = getSyn(word, "c9abab0abe60077362791f423bc3bedf");

                if (syns.Count > 0)
                {
                    m_synonymes.AddRange(syns);
                }

            }
            int R = calc_R(query/*, m_pathTo_qrels*/);
            creatDocsList(query,  docsLangDict, langsChosen); // create set of docNames and saves query's words recs from posting file,in a dict
            setQueryWordsData(query, m_postingRecOfWord);
            createDocsRanks(R, query, m_docsList);

           /*foreach (string doc in m_docsRanks.Keys)
            {
                Console.WriteLine("doc: " + doc + " rank: " + m_docsRanks[doc]);
            }*/

            docsForQuery = takeTopFifty(m_docsRanks);

            return docsForQuery;
        }
        //clean From StopWords
        private List<string> cleanFromStopWords(string query)
        {
            Regex rgx = new Regex("[^a-zA-Z0-9 -]"); // clean all non alphanumeric chars
            query = rgx.Replace(query, "");

            List<string> ans = new List<string>();
            string[] qyeryArray = query.Split(' ');
            foreach (string word in qyeryArray)
            {
                if (!m_stopWords.Contains(word))
                {
                    ans.Add(word);
                }
            }

            return ans;
        }
        /// <summary>
        /// gets synonyms for given word by using web server
        /// </summary>
        /// <param name="word"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        private static List<string> getSyn(string word, string key)
        {
            List<string> syn = new List<string>();

            try
            {
                WebRequest request = WebRequest.Create("http://words.bighugelabs.com/api/2/" + key + "/" + word + "/xml");
                WebResponse response = request.GetResponse();
                string res = ((HttpWebResponse)response).StatusDescription;
                Stream dataStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(dataStream);
                string responseFromServer = reader.ReadToEnd();
                reader.Close();
                response.Close();
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(responseFromServer);
                XmlNode node = doc.DocumentElement.SelectSingleNode("/words");

                foreach (XmlNode node_1 in doc.DocumentElement.ChildNodes)
                {
                    string text = node_1.InnerText;
                    syn.Add(text);
                }
            }
            catch (Exception)
            {
                Console.WriteLine("server failed: " + word);
            }
            return syn;
        }
        //takes top 50 docs
        private List<string> takeTopFifty(Dictionary<string, double> m_docsRanks) //sort and take top 50
        {
            List<string> ans = new List<string>();
            var sortedDict = (from entry in m_docsRanks orderby entry.Value descending select entry).Take(50)
                     .ToDictionary(pair => pair.Key, pair => pair.Value);
            foreach (KeyValuePair<string/*doc Id*/, double/*rank*/> item in sortedDict)
            {
                ans.Add(item.Key);
            }
            return ans;
        }
        //creat Docs List
        private void creatDocsList(string query,Dictionary<string/*doc*/,string/*lang*/> docsLangDict,List<string/*language*/> langsChosen) // create set of docNames and saves the words recs from posting in a dictionary
        {
            m_docsList = new HashSet<string/*doc ID*/>();
            m_postingRecOfWord = new Dictionary<string/*word*/, string/*posting rec*/>();
            long ptr = -1; // ptr to posting rec
            string rec = ""; // curr posting rec
            foreach (string word in m_queryWords)
            {
                if (m_Dictionary.ContainsKey(word))
                {
                    ptr = m_Dictionary[word];
                    rec = findPostingRec(m_pathToPostingFile, ptr);
                    m_postingRecOfWord.Add(word, rec);
                }

            }
            foreach (string syn in m_synonymes)
            {

                if (m_Dictionary.ContainsKey(syn))
                {
                    ptr = m_Dictionary[syn];
                    rec = findPostingRec(m_pathToPostingFile, ptr);
                    m_postingRecOfWord.Add(syn, rec);
                }
            }
            foreach (string keyword in m_postingRecOfWord.Keys)
            {
                //go on rec and add the doc name to the hashset
                string[] sRec = m_postingRecOfWord[keyword].Split(' ');
                for (int i = 0; i < sRec.Length; i++)
                {
                    if (sRec[i].EndsWith("#"))
                    {
                        string docID = sRec[i].Substring(0, sRec[i].Length - 1);
                        string currDocLang = docsLangDict[docID];
                        if(langsChosen == null || langsChosen.Count == 0) //user didn't chose any langs
                        {
                            m_docsList.Add(docID);
                        }
                        else
                        {
                            if (langsChosen.Contains(currDocLang)) //adds to list only docs with lnags that user chose
                            {
                                m_docsList.Add(docID);
                            }
                        }
                        

                    }
                }
            }
        }
        //find Posting Rec
        private string findPostingRec(string m_pathToPostingFile, long ptr) // find and return posting rec of certain word 
        {
            string rec = "";
            using (FileStream posFile = new FileStream(m_pathToPostingFile, FileMode.Open))
            {
                using (BinaryReader br = new BinaryReader(posFile))
                {
                    while (br.BaseStream.Position != br.BaseStream.Length)
                    {
                        if (ptr == 0)
                        {
                            rec = br.ReadString();
                            break;
                        }
                        else
                        {
                            br.ReadString();
                            if (ptr == br.BaseStream.Position)
                            {
                                rec = br.ReadString();
                                break;
                            }
                        }
                    }
                }
            }
            return rec;
        }

        //create Docs Ranks
        private void createDocsRanks(int R, string query, HashSet<string/*doc Id*/> docsList) // rank the docs using Ranker, saves in dictionary
        {
            // creatMatrixForLSI();
            m_docsRanks = new Dictionary<string/*doc Id*/, double/*rank*/>();
            double currRank;
            foreach (string doc in m_docsList)
            {
                int dl = takeDocsLength(doc);
                currRank = Ranker.rankDoc(R, m_N, m_k1, m_k2, m_b, m_queryWordsData, dl, m_avdl, doc, m_q, m_Dk_T, m_docsMap);
                m_docsRanks.Add(doc, currRank);
            }

        }
        private void createWordsMap() //func for LSI
        {
            m_wordsMap = new Dictionary<string, int>();
            int row = 0;
            foreach (string word in m_postingRecOfWord.Keys)
            {
                m_wordsMap.Add(word, row);
                row++;
            }
        }
        private void createDocsMap() //func for LSI
        {
            m_docsMap = new Dictionary<string, int>();
            int col = 0;
            foreach (string doc in m_docsList)
            {
                m_docsMap.Add(doc, col);
                col++;
            }
        }
        private void creatMatrixForLSI()//func for LSI
        {
            createWordsMap();
            createDocsMap();
            int rows = m_wordsMap.Count;
            int cols = m_docsMap.Count;
            double[,] A = new double[rows, cols];
            double[,] q = new double[rows, 1];
            foreach (string word in m_wordsMap.Keys) //create A matrix
            {
                long ptr = m_Dictionary[word];
                string rec = findPostingRec(@"C:\Users\Daniel\Documents\Visual Studio 2015\Projects\IR\IR\bin\Debug\t\PostingF.txt", ptr);
                List<string> docsOfword = new List<string>();
                string[] sRec = rec.Split(' ');
                for (int i = 0; i < sRec.Length; i++)
                {
                    if (sRec[i].EndsWith("#"))
                    {
                        docsOfword.Add(sRec[i].Substring(0, sRec[i].Length - 1));
                    }
                }
                int Arow = m_wordsMap[word];
                foreach (string doc in docsOfword)
                {
                    int Acol = m_docsMap[doc];
                    A[Arow, Acol] = 1;
                }

                q[Arow, 0] = 1; //set q vector
            }
            var Amatrix = DenseMatrix.OfArray(A);
            var svd = Amatrix.Svd(true);
            var D_T = svd.VT;
            var T = svd.U;
            var S = svd.W;
            int k = 1;
            var D_Tk = D_T.SubMatrix(0, k, 0, D_T.ColumnCount);

            var Tk = T.SubMatrix(0, T.RowCount, 0, k);
            var Sk = S.SubMatrix(0, k, 0, k);
            var Sk_inver = Sk.Inverse();

            m_Dk_T = D_Tk;
            m_Sk_invers = Sk_inver;
            m_Tk = Tk;

            var Qmatrix = DenseMatrix.OfArray(q);
            var q_T = Qmatrix.Transpose();
            var qMatrix = q_T * m_Tk * m_Sk_invers;
            m_q = qMatrix.Row(0);
        }

        private int takeDocsLength(string doc) //takes docs length from ReadFile data struct & calculate avdl
        {
            int ans = 0;

            ans = m_docsLength[doc];

            return ans;
        }

        private int calc_R(string query/*, string m_pathTo_qrels*/) // calcs num of relevant docs to query
        {
            int R = 0;

            //clac R if possible from file qrels

            return R;
        }

        private void setQueryWordsData(string query, Dictionary<string/*word*/, string/*posting rec*/> postingRecOfWord)
        {
            m_queryWordsData = new Dictionary<string/*word in query*/, queryWordData>();

            foreach (string word in m_queryWords)
            {
                queryWordData data = new queryWordData();
                data.r_i = 0;
                data.n_i = calc_n_i(word);
                data.qf_i = calc_qf_i(word, query);
                data.f_i = create_DocId_Fi(postingRecOfWord, word); //new Dictionary<string/*docId*/, int/*df*/>();

                m_queryWordsData.Add(word, data);
            }
        }

        private Dictionary<string/*docID*/, int/*fi*/> create_DocId_Fi(Dictionary<string, string> postingRecOfWord, string word) //find f-i for each word-doc combination
        {
            Dictionary<string/*doc ID*/, int/*fi*/> ans = new Dictionary<string, int>();
            if (!postingRecOfWord.ContainsKey(word))
            {
                ans.Add("", 0);
                return ans;
            }
            string rec = postingRecOfWord[word];

            string[] sRec = rec.Split(' ');
            for (int i = 0; i < sRec.Length; i++)
            {
                int f_i = 0;
                if (sRec[i].EndsWith("#"))
                {
                    string docName = sRec[i].Substring(0, sRec[i].Length - 1);
                    for (int j = i + 1; j < sRec.Length && sRec[j] != "|"; j++)
                    {
                        f_i++;
                    }

                    ans.Add(docName, f_i);

                }
            }
            return ans;
        }

        private int calc_qf_i(string word, string query)
        {
            int qf_i = 0;
            string[] sQuery = query.Split(' ');
            for (int i = 0; i < sQuery.Length; i++)
            {
                if (sQuery[i] == word)
                {
                    qf_i++;
                }
            }
            return qf_i;
        }

        //calculate n_i (DF) for a specific word from the query
        private int calc_n_i(string word)
        {
            int n_i = 0;
            try
            {
                string rec = m_postingRecOfWord[word];
                string[] sRec = rec.Split(' ');
                for (int i = 0; i < sRec.Length; i++)
                {
                    if (sRec[i].EndsWith("#"))
                    {
                        n_i++;
                    }
                }
            }
            catch (Exception)
            {
                Console.WriteLine("the word " + word + " wasn't in posting");
            }
            return n_i;
        }
    }
}
