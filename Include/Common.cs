namespace QnA.Include
{
    public class Common
    {
        #region ChangeDate(날짜)-날짜 형식_0000.00.00
        ///<summary>
        ///0000-00-00를 0000.00.00 형식으로 변경
        ///</summary>
        ///<param name="objDate">2020-01-01 12:11:11</param>
        ///<returns>2020.01.01</returns>
        public static string ChangeDate(object objDate)
        {
            if (objDate == null)
            {
                return "-";
            }
            else
            {
                if (!string.IsNullOrEmpty(objDate.ToString()))
                {
                    string strRegDate = Convert.ToDateTime(objDate).ToString("yyyy-MM-dd");
                    return Convert.ToDateTime(objDate).ToString().Substring(0, 10).Replace("-", ".");
                }
                else
                {
                    return "-";
                }
            }
        }
        #endregion

        #region FuncNew(날짜) - 새 글 여부
        ///<summary>
        ///24시간 이내 올라온 글일 경우 new 아이콘 표시
        /// </summary>
        /// <param name="objDate">2020-01-01 12:12:12</param>
        /// <returns><img src="/images/new.gif"></returns>
        public static string FuncNew(object objDate)
        {
            if (objDate != null)
            {
                if (!string.IsNullOrEmpty(objDate.ToString()))
                {
                    DateTime originDate = Convert.ToDateTime(objDate);

                    TimeSpan objTs = DateTime.Now - originDate;

                    string newImage = "";
                    if (objTs.TotalMinutes < 1440)
                    {
                        newImage = "<img src=\"/images/new.gif\">";
                    }
                    return newImage;
                }
            }

            return "";
        }
        #endregion

        #region 각 글의 Step별 들여쓰기 처리
        /// <summary>
        /// 각 글의 Step별 들여쓰기 처리
        /// </summary>
        /// <param name="objStep">1, 2, 3</param>
        /// <returns>Re 이미지를 포함한 Step만큼 들여쓰기</returns>
        public static string FuncStep(object objLevel)
        {
            int intLevel = Convert.ToInt32(objLevel);
            string strTemp = String.Empty;

            if (intLevel != 0)
            {
                if (intLevel == 1)
                {
                    strTemp = String.Format("<img src=\"/images/blank.gif\" height=\"0\" width=\"15\"><img src=\"/images/re.png\">");
                }
                else
                {
                    for (int i = 0; i < intLevel; i++)
                    {
                        strTemp = String.Format("<img src=\"{0}\" height=\"{1}\" width=\"{2}\">", "/images/blank.gif", "0", (intLevel * 15));
                    }
                    strTemp += "<img src=\"/images/re.png\">";
                }
            }

            return strTemp;
        }
        #endregion
    }
}
