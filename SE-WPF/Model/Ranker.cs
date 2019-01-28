using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SE_WPF.Model
{
    /// <summary>
    /// This class reprsents a ranker that ranks a given document
    /// </summary>
    class Ranker
    {
        /// <summary>
        /// ranks a given document using semantic algorithem and BM25 algorithem
        /// </summary>
        /// <param name="R"></param>
        /// <param name="N"></param>
        /// <param name="k1"></param>
        /// <param name="k2"></param>
        /// <param name="b"></param>
        /// <param name="queryWordsData"></param>
        /// <param name="dl"></param>
        /// <param name="avdl"></param>
        /// <param name="docId"></param>
        /// <param name="q"></param>
        /// <param name="Dk_T"></param>
        /// <param name="docsMap"></param>
        /// <returns></returns>
        public static double rankDoc(int R, int N, double k1, double k2, double b, Dictionary<string/*word in query*/, queryWordData/*struct of data*/> queryWordsData, double dl, double avdl, string docId, MathNet.Numerics.LinearAlgebra.Vector<double> q, MathNet.Numerics.LinearAlgebra.Matrix<double> Dk_T, Dictionary<string, int> docsMap)
        {
            double rank;
            rank = BM25(R, N, k1, k2, b, queryWordsData, dl, avdl, docId);/*LSI(docId,q, Dk_T, docsMap);*/
            return rank;
        }
        //LSI ranker
        private static double LSI(string doc, Vector<double> q, Matrix<double> Dk_T, Dictionary<string, int> docsMap)
        {
            double score_D_Q = 0;

            int docsCol = docsMap[doc];
            Vector<double> dCurr = Dk_T.Column(docsCol); //take the vector of the curr doc
            double mone = q.DotProduct(dCurr);
            double mahane = q.Norm(2) * dCurr.Norm(2);
            score_D_Q = mone / mahane;

            return score_D_Q;

        }
        //BM25 ranker
        private static double BM25(int R, int N, double k1, double k2, double b, Dictionary<string/*word in query*/, queryWordData/*struct of data*/> queryWordsData, double dl, double avdl, string docId)
        {
            double score_D_Q = 0;
            double score_D_Q_i;
            int r_i, n_i, qf_i, f_i;

            double K = k1 * ((1 - b) + b * (dl / avdl));

            foreach (string word in queryWordsData.Keys)
            {
                r_i = queryWordsData[word].r_i;
                n_i = queryWordsData[word].n_i;
                qf_i = queryWordsData[word].qf_i;
                if (queryWordsData[word].f_i.ContainsKey(docId))
                {
                    f_i = queryWordsData[word].f_i[docId];
                }
                else // the curr word is not in curr doc
                {
                    f_i = 0;
                }

                double inLog = ((r_i + 0.5) / (R - r_i + 0.5)) / ((n_i - r_i + 0.5) / (N - n_i - R + r_i + 0.5));
                double m = ((k1 + 1) * f_i) / (K + f_i);
                double n = ((k2 + 1) * qf_i) / (k2 + qf_i);
                score_D_Q_i = (Math.Log(inLog, 2)) * m * n;

                score_D_Q += score_D_Q_i;
            }
            return score_D_Q;
        }
    }
}
