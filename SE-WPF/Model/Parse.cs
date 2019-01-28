using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SE_WPF.Model
{
    /// <summary>
    /// This class represents a parser that parse documents from a given corpus
    /// </summary>
    class Parse
    {
        private HashSet<string> m_stopWords;
        private string[] textArray;
        private List<List<string/*parsed term*/>> m_parsedTermsOfFile; //each inner list is list of parsed terms of single doc
        string m_currDocID; //holds curr doc ID
        string m_currDocLang; //holds curr doc language
        List<string> m_parsedTermsOfDoc; //holds parsed terms of curr doc
        Stemmer m_stemmer;
        bool m_stemOn;
        //int m_i;

        /// <summary>
        /// c'tor
        /// </summary>
        /// <param name="pathToStopWords"></param>
        /// <param name="stemOn"></param>
        public Parse(string pathToStopWords, bool stemOn)
        {
            m_stopWords = makeStopWordsList(pathToStopWords);
            m_stemmer = new Stemmer();
            m_stemOn = stemOn;
            //m_i = 0;
        }
        /// <summary>
        /// inits list of parsed terms of file
        /// </summary>
        public void InitParser() 
        {
            m_parsedTermsOfFile = new List<List<string>>();
        }
        /// <summary>
        /// Make parsed terms
        /// </summary>
        /// <param name="docs">documents</param>
        /// <returns></returns>
        public List<List<string/*parsed term*/>> MakeTerms(List<string> docs)
        {
            int docIDstrIndx; int docIDendIndx;
            int docLangStrIndx; int docLangEndIndx;
            int docTextStrUndx; int docTextEndIndx;
            string text_Of_Curr_Doc;
            foreach (string doc in docs)
            {
                //take doc name
                docIDstrIndx = doc.IndexOf("<DOCNO>") + 7;
                docIDendIndx = doc.IndexOf("</DOCNO>");
                m_currDocID = doc.Substring(docIDstrIndx, docIDendIndx - docIDstrIndx);
                m_currDocID = Regex.Replace(m_currDocID, @"\s+", ""); //removes empty spaces
                //take doc language
                docLangStrIndx = doc.IndexOf("<F P=105>") + 8;
                docLangEndIndx = doc.IndexOf("</F>", docLangStrIndx);
                if (docLangStrIndx != -1 && docLangEndIndx != -1)
                {
                    m_currDocLang = doc.Substring(docLangStrIndx, docLangEndIndx - docLangStrIndx);
                    Regex.Replace(m_currDocLang, " ", string.Empty);
                    
                }
                else
                    m_currDocLang = "UNKNOWN";

                docTextStrUndx = doc.IndexOf("<TEXT>") + 6;
                docTextEndIndx = doc.IndexOf("</TEXT>");

                text_Of_Curr_Doc = doc.Substring(docTextStrUndx, docTextEndIndx - docTextStrUndx);

                ParseCurrDocText(text_Of_Curr_Doc);
                //m_i++;
                //Console.WriteLine(m_i);
            }
            return m_parsedTermsOfFile;
        }
        /// <summary>
        /// parse current doc text
        /// </summary>
        /// <param name="text_Of_Curr_Doc"></param>
        private void ParseCurrDocText(string text_Of_Curr_Doc)
        {
            m_parsedTermsOfDoc = new List<string>();

            char[] delimiterChars = { ' ', '\t', '\n', '\r' };

            textArray = text_Of_Curr_Doc.ToUpper().Split(delimiterChars, System.StringSplitOptions.RemoveEmptyEntries);//split the terms by space at the curr line and make them in upper case

            textArray = CleenTerms(textArray); // cleen the terms from ',' ,':' ...
                                               //ToDo: cleen dots but considering term like u.s. u.s.a etc
            int i = 0;
            for (i = 0; i < textArray.Length; i++)
            {

                if (RuleTen(ref i)) //100 billion u.s. dollars --> 100000 M dollars
                {
                    continue;
                }
                if (RuleFour(ref i)) //leave between <num> and <num> as is
                {
                    continue;
                }
                if (RuleThirteen(ref i))  //16th may 1991 --> 1991-05-16
                {
                    continue;
                }
                if (RuleTwelve(ref i))  // 14 may 91 --> 1991-05-14
                {
                    continue;
                }
                if (RuleEleven(ref i)) // may 16, 1991 --> 1991-05-16  (Month DD, YYYY)
                {
                    continue;
                }
                if (RuleSeven(ref i)) //leave <num> <fraction> Dollar as is
                {
                    continue;
                }
                if (RuleSixteen(ref i)) // may 1991 --> 1991-05
                {
                    continue;
                }
                if (RuleFifteen(ref i)) // june 14 --> 06-14
                {
                    continue;
                }
                if (RuleFourteen(ref i))  //14 may --> 05-14
                {
                    continue;
                }
                if (RuleNine(ref i))  //$100 million --> 100 M dollars
                {
                    continue;
                }
                if (RuleSix(ref i))  //leave <num> DOLLARS as is
                {
                    continue;
                }
                if (RuleFive(ref i)) //<num> percent --> <num>% , <num> percentage --> <num>%
                {
                    continue;
                }
                if (RuleThree(ref i)) // leave nums with fractions as is ( 1 1/2 ---> 1 1/2)
                {
                    continue;
                }
                if (RuleTwo(ref i))  //7 Million --> 7M etc...
                {
                    continue;
                }
                if (RuleOne(ref i)) // RuleOne: 1,000,000 ---> 1M , numbers smaller than 1M stay as they were
                {
                    continue;
                }
                if (RuleEight(ref i)) //$<num> ---> <num> Dollars  (num < 1,000,000)
                {
                    continue;
                }
                if (m_stopWords.Contains(textArray[i])) continue;

                
                if (textArray[i] != null && textArray[i] !=string.Empty && textArray[i] !="")//dont enter to list empty strings
                {
                    addToParsedList(textArray[i]);
                }

            }

            //extract meta data
            string maxRepeatedTerm = m_parsedTermsOfDoc.GroupBy(s => s)
                                    .OrderByDescending(s => s.Count())
                                    .First().Key;
            int num_Of_Occurences_Of_Max_Term = m_parsedTermsOfDoc.Where(s => s == maxRepeatedTerm).Count();

            string minRepeatedTerm = m_parsedTermsOfDoc.GroupBy(s => s)
                                    .OrderBy(s => s.Count())
                                    .First().Key;
            int num_Of_Occurences_Of_Min_Term = m_parsedTermsOfDoc.Where(s => s == minRepeatedTerm).Count();

            //add meta data (docID, lang...)
            m_parsedTermsOfDoc.Insert(0, m_currDocID); //add docID at the top of the list (0)
            m_parsedTermsOfDoc.Insert(1, m_currDocLang); //add doc language at the second place of the list (1)
            m_parsedTermsOfDoc.Insert(2, num_Of_Occurences_Of_Max_Term.ToString()); //max_tf
            m_parsedTermsOfDoc.Insert(3, num_Of_Occurences_Of_Min_Term.ToString()); //num of unique terms

            //add parsed list of the curr doc to the big list
            m_parsedTermsOfFile.Add(m_parsedTermsOfDoc);
        }
        
        private void addToParsedList(string term) //adds to term to persed list stemmed or not according to flag stemOn
        {
            if (m_stemOn)
            {
                term = term.ToLower();
                term = m_stemmer.stemTerm(term);
                term = term.ToUpper();
            }
            m_parsedTermsOfDoc.Add(term);
        }

        // may 1991 --> 1991-05
        private bool RuleSixteen(ref int i)
        {
            if (i + 1 >= textArray.Length)
            {
                return false;
            }

            if (checkIfYear(textArray[i + 1]) && checkIfMonth(textArray[i]))
            {
                addToParsedList(textArray[i + 1] + "-" + convertMonth(textArray[i]));
                i += 1;
                return true;
            }
            return false;
        }


        // 3 --> 03
        private string convertDay(string d)
        {
            string ans = d;
            if (d.Length == 1)
            {
                ans = "0" + d;
            }
            return ans;
        }

        // june 14 --> 06-14
        private bool RuleFifteen(ref int i)
        {
            if (i + 1 >= textArray.Length)
            {
                return false;
            }

            if (checkIfDay(textArray[i + 1]) && checkIfMonth(textArray[i]))
            {
                string day = convertDay(textArray[i + 1]);
                addToParsedList(convertMonth(textArray[i]) + "-" + day);
                i += 1;
                return true;
            }


            return false;
        }



        //14 may --> 05-14
        private bool RuleFourteen(ref int i)
        {
            if (i + 1 >= textArray.Length)
            {
                return false;
            }

            if (checkIfDay(textArray[i]) && checkIfMonth(textArray[i + 1]))
            {
                string day = convertDay(textArray[i]);
                addToParsedList(day + "-" + convertMonth(textArray[i + 1]));
                i += 1;
                return true;
            }

            return false;
        }


        //16th may 1991 --> 1991-05-16
        private bool RuleThirteen(ref int i)
        {
            if (i + 2 >= textArray.Length)
            {
                return false;
            }

            if (checkIfYear(textArray[i + 2]))
            {
                if (checkIfDayTH(textArray[i]) && checkIfMonth(textArray[i + 1]))
                {
                    string day = textArray[i].Replace("TH", string.Empty);
                    day = convertDay(day);
                    addToParsedList(textArray[i + 2] + "-" + convertMonth(textArray[i + 1]) + "-" + day);
                    i += 2;
                    return true;
                }

            }
            return false;
        }


        private bool checkIfDayTH(string v)
        {
            if (v.EndsWith("TH"))
            {
                v = v.Replace("TH", string.Empty);
                if (checkIfDay(v))
                {
                    return true;
                }
                return false;
            }
            return false;
        }

        // 14 may 91 --> 1991-05-14
        private bool RuleTwelve(ref int i)
        {
            if (i + 2 >= textArray.Length)
            {
                return false;
            }

            if (checkIfShortYear(textArray[i + 2]))
            {
                if (checkIfDay(textArray[i]) && checkIfMonth(textArray[i + 1]))
                {

                    int num_year = Int32.Parse(textArray[i + 2]);
                    string year;
                    if (num_year >= 0 && num_year <= 20)
                    {
                        year = "20" + textArray[i + 2];
                    }
                    else year = "19" + textArray[i + 2];

                    addToParsedList(year + "-" + convertMonth(textArray[i + 1]) + "-" + convertDay(textArray[i]));
                    i += 2;
                    return true;
                }
            }
            return false;
        }

        private bool checkIfShortYear(string year)
        {
            int num;
            if (year.Length == 2 && Int32.TryParse(year, out num))
            {
                return true;
            }
            return false;
        }

        // may 16, 1991 --> 1991-05-16  (Month DD, YYYY)
        private bool RuleEleven(ref int i)
        {
            if (i + 2 >= textArray.Length)
            {
                return false;
            }
            if (checkIfYear(textArray[i + 2]))
            {
                if (checkIfDay(textArray[i + 1]) && checkIfMonth(textArray[i]))
                {
                    addToParsedList(textArray[i + 2] + "-" + convertMonth(textArray[i]) + "-" + convertDay(textArray[i + 1]));
                    i += 2;
                    return true;
                }
                if (checkIfDay(textArray[i]) && checkIfMonth(textArray[i + 1]))
                {
                    addToParsedList(textArray[i + 2] + "-" + convertMonth(textArray[i + 1]) + "-" + convertDay(textArray[i]));
                    i += 2;
                    return true;
                }
            }
            return false;
        }


        private bool checkIfMonth(string month)
        {
            string[] monthes = { "JAN","JANUARY", "FEB","FEBRUARY", "MAR","MARCH"
                    , "APR","APRIL", "MAY", "JUN","JUNE", "JUL", "JULY", "AUG","AUGUST"
                    , "SEP", "SEPTEMBER", "OCT","OCTOBER", "NOV","NOVEMBER", "DEC","DECEMBER" };
            foreach (string s in monthes)
            {
                if (month == s)
                {
                    return true;
                }
            }
            return false;
        }

        private bool checkIfYear(string s)
        {

            int year;
            if (Int32.TryParse(s, out year) && s.Length == 4)
            {
                return true;
            }
            return false;
        }

        private bool checkIfDay(string s)
        {
            int day;
            if (Int32.TryParse(s, out day) && day > 0 && day < 32)
            {
                return true;
            }
            return false;
        }

        private string convertMonth(string s)
        {
            string month = "";
            switch (s)
            {
                case "JANUARY":
                case "JAN":
                    month = "01";
                    break;
                case "FEBRUARY":
                case "FEB":
                    month = "02";
                    break;
                case "MARCH":
                case "MAR":
                    month = "03";
                    break;
                case "APRIL":
                case "APR":
                    month = "04";
                    break;
                case "MAY":
                    month = "05";
                    break;
                case "JUNE":
                case "JUN":
                    month = "06";
                    break;
                case "JUL":
                case "JULY":
                    month = "07";
                    break;
                case "AUGUST":
                case "AUG":
                    month = "08";
                    break;
                case "SEPTEMBER":
                case "SEP":
                    month = "09";
                    break;
                case "OCTOBER":
                case "OCT":
                    month = "10";
                    break;
                case "NOVEMBER":
                case "NOV":
                    month = "11";
                    break;
                case "DECEMBER":
                case "DEC":
                    month = "12";
                    break;
            }
            return month;
        }

        //100 billion u.s. dollars --> 100000 M dollars
        private bool RuleTen(ref int i)
        {
            decimal num;
            if ((i + 3) >= textArray.Length)
            {
                return false;
            }
            if (textArray[i + 3] == "DOLLARS" && textArray[i + 2] == "U.S." && Decimal.TryParse(textArray[i], out num))
            {
                if (textArray[i + 1] == "MILLION")
                {
                    addToParsedList(textArray[i] + " M DOLLARS");
                    i += 3;
                    return true;
                }
                if (textArray[i + 1] == "BILLION")
                {
                    addToParsedList((num * 1000).ToString() + " M DOLLARS");
                    i += 3;
                    return true;
                }
                if (textArray[i + 1] == "TRILLION")
                {
                    addToParsedList((num * 1000000).ToString() + " M DOLLARS");
                    i += 3;
                    return true;
                }
            }
            return false;
        }

        //$100 million --> 100 M dollars
        private bool RuleNine(ref int i)
        {
            if (i + 1 >= textArray.Length)
            {
                return false;
            }
            decimal num;
            if (textArray[i].Length <= 1) return false;
            if (textArray[i][0] == '$' && Decimal.TryParse(textArray[i].Substring(1, textArray[i].Length - 1), out num))
            {
                if (textArray[i + 1] == "MILLION")
                {
                    addToParsedList(num + " M" + " DOLLARS");
                    i++;
                    return true;
                }
                if (textArray[i + 1] == "BILLION")
                {
                    addToParsedList((num * 1000).ToString() + " M" + " DOLLARS");
                    i++;
                    return true;
                }
                if (textArray[i + 1] == "TRILLION")
                {
                    addToParsedList((num * 1000000).ToString() + " M" + " DOLLARS");
                    i++;
                    return true;
                }
                if (textArray[i + 1] == "QUADRILLION")
                {
                    addToParsedList((num * 1000000000).ToString() + " M" + " DOLLARS");
                    i++;
                    return true;
                }
            }
            return false;
        }

        //leave <num> <fraction> Dollar as is
        private bool RuleSeven(ref int i)
        {
            if (i + 2 >= textArray.Length)
            {
                return false;
            }

            int num;
            if (textArray[i + 2] == "DOLLARS" && Regex.IsMatch(textArray[i + 1], @"\d+(\.\d+)?/\d+(\.\d+)?") && Int32.TryParse(textArray[i], out num))
            {
                addToParsedList(textArray[i] + " " + textArray[i + 1] + " " + textArray[i + 2]);
                i += 2;
                return true;
            }
            return false;
        }


        //leave <num> DOLLARS as is
        private bool RuleSix(ref int i)
        {
            if (i + 1 >= textArray.Length)
            {
                return false;
            }


            if (textArray[i + 1] == "DOLLARS")
            {
                decimal num;

                if (Decimal.TryParse(textArray[i], out num))
                {
                    string t;
                    if (num >= 1000000)
                        t = (num / 1000000 + " M DOLLARS");
                    else
                        t = (num + " DOLLARS");
                    addToParsedList(t);
                    i += 1;
                    return true;
                }
                if (textArray[i].Length >= 1 && textArray[i].Substring(textArray[i].Length - 1, 1) == "M")//20.6M dollars --> 20.6 M dollars
                {
                    addToParsedList(textArray[i].Substring(0, textArray[i].Length - 1) + " M " + textArray[i + 1]);
                    i += 1;
                    return true;
                }

                if (textArray[i].Length >= 2 && textArray[i].Substring(textArray[i].Length - 2, 2) == "BN" && Decimal.TryParse(textArray[i].Substring(0, textArray[i].Length - 2), out num))//20bn dollars --> 20000 M dollars
                {
                    addToParsedList((num * 1000).ToString() + " M " + textArray[i + 1]);
                    i += 1;
                    return true;
                }

            }
            return false;
        }


        //<num> percent --> <num>% , <num> percentage --> <num>%
        private bool RuleFive(ref int i)
        {
            if (i + 1 >= textArray.Length)
            {
                return false;
            }

            decimal num;
            if ((textArray[i + 1] == "PERCENT" || textArray[i + 1] == "PERCENTAGE") && Decimal.TryParse(textArray[i], out num))
            {
                addToParsedList(textArray[i] + "%");
                i += 1;
                return true;
            }
            return false;
        }


        //leave between <num> and <num> as is
        private bool RuleFour(ref int i)
        {
            if (i + 3 >= textArray.Length)
            {
                return false;
            }

            int num;
            if (textArray[i] == "BETWEEN" && textArray[i + 2] == "AND" &&
                Int32.TryParse(textArray[i + 1], out num) && Int32.TryParse(textArray[i + 3], out num))
            {
                addToParsedList(textArray[i] + " " + textArray[i + 1] + " " + textArray[i + 2] + " " + textArray[i + 3]);
                i += 3;
                return true;
            }
            return false;
        }


        // leave nums with fractions as is ( 1 1/2 ---> 1 1/2)
        private bool RuleThree(ref int i)
        {
            if (i + 1 >= textArray.Length)
            {
                return false;
            }

            int num;
            if (Regex.IsMatch(textArray[i + 1], @"\d+(\.\d+)?/\d+(\.\d+)?") && Int32.TryParse(textArray[i], out num))
            {
                addToParsedList(textArray[i] + " " + textArray[i + 1]);
                i += 1;
                return true;
            }
            return false;
        }

        //7 Million --> 7M etc...
        private bool RuleTwo(ref int i)
        {
            if ((i + 1) >= textArray.Length)
            {
                return false;
            }
            decimal num;
            if (Decimal.TryParse(textArray[i], out num))
            {
                if (textArray[i + 1] == "MILLION")
                {
                    addToParsedList(num.ToString() + "M");
                    i++;
                    return true;
                }

                if (textArray[i + 1] == "BILLION")
                {
                    addToParsedList((num * 1000).ToString() + "M");
                    i++;
                    return true;
                }
                if (textArray[i + 1] == "TRILLION")
                {
                    addToParsedList((num * 1000000).ToString() + "M");
                    i++;
                    return true;
                }
                if (textArray[i + 1] == "QUADRILLION")
                {
                    addToParsedList((num * 1000000000).ToString() + "M");
                    i++;
                    return true;
                }
            }
            return false;
        }

        //removes unnecessary chars from the terms
        private string[] CleenTerms(string[] terms)
        {
            //List<string> cleenTerms = new List<string>();
            for (int i = 0; i < terms.Length; i++)
            {
                terms[i] = terms[i].Replace(":", string.Empty);
                if (terms[i].EndsWith(",")) //to remove comma only when <word>, and not for numbers like 1,000
                { terms[i] = terms[i].Replace(",", string.Empty); }
                terms[i] = terms[i].Replace("+", string.Empty); terms[i] = terms[i].Replace("&", string.Empty);
                terms[i] = terms[i].Replace("*", string.Empty); terms[i] = terms[i].Replace("_", string.Empty);
                terms[i] = terms[i].Replace("(", string.Empty); terms[i] = terms[i].Replace(":", string.Empty);
                terms[i] = terms[i].Replace(")", string.Empty); terms[i] = terms[i].Replace("`", string.Empty);
                terms[i] = terms[i].Replace("\"", string.Empty); terms[i] = terms[i].Replace("�", string.Empty);
                terms[i] = terms[i].Replace("?", string.Empty); terms[i] = terms[i].Replace("]", string.Empty);
                terms[i] = terms[i].Replace(";", string.Empty); terms[i] = terms[i].Replace("[", string.Empty);
                terms[i] = terms[i].Replace("!", string.Empty); terms[i] = terms[i].Replace("{", string.Empty);
                terms[i] = terms[i].Replace("@", string.Empty); terms[i] = terms[i].Replace("}", string.Empty);
                terms[i] = terms[i].Replace("#", string.Empty); terms[i] = terms[i].Replace("'", string.Empty);
                terms[i] = terms[i].Replace("^", string.Empty); terms[i] = terms[i].Replace("|", string.Empty);
                if (terms[i].EndsWith("-")) //to remove - only when <word>- and not for frases like united-states
                { terms[i] = terms[i].Replace("-", string.Empty); }
                if ((terms[i].EndsWith(".") && terms[i] != "U.S.") || terms[i].StartsWith(".")) //to remove dot only when <word>. 
                { terms[i] = terms[i].Replace(".", string.Empty); }
                if (terms[i].Length == 1 && terms[i] == "$")
                {
                    terms[i] = terms[i].Replace("$", string.Empty);
                }
                //ToDo add rules

            }
            return terms;
        }

        //$<num> ---> <num> Dollars  (num < 1,000,000)
        private bool RuleEight(ref int i)
        {
            decimal num;
            if (textArray[i].Length <= 1) return false;
            if (textArray[i][0] == '$' && Decimal.TryParse(textArray[i].Substring(1, textArray[i].Length - 1), out num))
            {
                addToParsedList(num + " DOLLARS");
                return true;
            }
            return false;
        }


        // RuleOne: 1,000,000 ---> 1M , numbers smaller than 1M stay as they were
        private bool RuleOne(ref int i)
        {
            decimal num;
            if (Decimal.TryParse(textArray[i], out num) && num >= 1000000)
            {
                addToParsedList(num / 1000000 + "M");
                return true;
            }
            return false;
        }

        private bool IsDigitsOnly(string str)
        {

            foreach (char c in str)
            {
                if (c < '0' || c > '9')
                    return false;
            }
            return true;
        }



        //makes stop words list from a given file (Enter them in upper case form)
        private HashSet<string> makeStopWordsList(string path)
        {
            StreamReader file = new StreamReader(path);
            HashSet<string> stopWords = new HashSet<string>();
            string line;
            while ((line = file.ReadLine()) != null)
            {
                line = line.ToUpper();
                stopWords.Add(line);
            }
            return stopWords;
        }
    }
}
