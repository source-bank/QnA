using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using QnA.Include;
using QnA.Models.Tables;
using System;
using System.Data;

namespace QnA.Models
{
    public class DAL
    {
        private readonly DBConnectionContext _connectionContext;
        public DAL(DBConnectionContext connectionContext)
        {
            _connectionContext = connectionContext;
        }

        #region 게시글 수 - GetQnACount()
        /// <summary>
        /// Q&A에 등록된 게시글 수를 가져온다.
        /// <param name="searchCondition">검색조건</param>
        /// <param name="SearchKeyword">검색어</param>
        /// <returns>게시글 수</returns>
        /// </summary>
        public int GetQnACount(string searchCondition, string searchKeyword)
        {
            SqlConnection db = _connectionContext.Database.GetDbConnection() as SqlConnection;
            db.Open();

            int AllCnt = 0;
            using (SqlCommand cmd = db.CreateCommand())
            {
                string SQL = @"
                    SELECT
	                    COUNT(*)
                    FROM
	                    QnA
                    WHERE
	                    DelDiv = 0
                ";
                if (!string.IsNullOrEmpty(searchKeyword.ToString()))
                {
                    switch (searchCondition.ToString())
                    {
                        case "1":
                            SQL += @"	AND BTitle LIKE @searchKeyword";
                            break;
                        case "2":
                            SQL += @"	AND BContent LIKE @searchKeyword";
                            break;
                        case "3":
                            SQL += @"	AND UserName LIKE @searchKeyword";
                            break;
                        default:
                            break;
                    }
                }
                cmd.CommandText = SQL;
                cmd.CommandType = System.Data.CommandType.Text;
                if (!string.IsNullOrEmpty(searchKeyword.ToString()))
                {
                    cmd.Parameters.AddWithValue("@searchKeyword", "%" + searchKeyword.ToString() + "%");
                }

                AllCnt = Convert.ToInt32(cmd.ExecuteScalar());
            }

            db.Close();

            return AllCnt;
        }
        #endregion

        #region 게시글 - GetQnAList()
        ///<summary>
        ///Q&A에 등록된 게시글을 가져온다.
        ///<param name="searchCondition">검색조건</param>
        ///<param name="searchKeyword">검색어</param>
        ///<returns>게시글</returns>
        /// </summary>
        public DataTable GetQnAList(int page, string searchCondition, string searchKeyword)
        {
            SqlConnection db = _connectionContext.Database.GetDbConnection() as SqlConnection;
            db.Open();

            DataTable dt = new DataTable();

            using (SqlCommand cmd = db.CreateCommand())
            {
                string SQL = @"
                    WITH List
                    AS
                    (
                        SELECT
	                        ROW_NUMBER() OVER (ORDER BY BRef DESC, BStep) AS 'RowNum',
	                        Idx,
	                        BRef,
	                        BLevel,
	                        BStep,
	                        BTitle,
	                        UserName,
	                        UserPwd,
	                        BContent,
	                        HitCount,
	                        RegDate,
	                        DelDiv
                        FROM
	                        QnA
                        WHERE
	                        DelDiv = 0
                ";
                if (!string.IsNullOrEmpty(searchKeyword.ToString()))
                {
                    switch (searchCondition.ToString())
                    {
                        case "1":
                            SQL += @"	AND BTitle LIKE @searchKeyword";
                            break;
                        case "2":
                            SQL += @"	AND BContent LIKE @searchKeyword";
                            break;
                        case "3":
                            SQL += @"	AND UserName LIKE @searchKeyword";
                            break;
                        default:
                            break;
                    }
                }
                SQL += @")
                    SELECT
	                    *
                    FROM
	                    List
                    WHERE
	                    RowNum BETWEEN @page * 10 + 1 AND (@page + 1) * 10
                ";
                cmd.CommandText = SQL;
                cmd.CommandType = System.Data.CommandType.Text;
                cmd.Parameters.AddWithValue("@page", page);
                if (!string.IsNullOrEmpty(searchKeyword.ToString()))
                {
                    cmd.Parameters.AddWithValue("@searchKeyword", "%" + searchKeyword.ToString() + "%");
                }

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);
            }

            db.Close();

            return dt;
        }
        #endregion

        #region 글 등록 - SetQnAWriteOk()
        ///<summary>
        ///Q&A에 글을 등록한다.
        ///<param name="model">글 작성 시 입력폼에 작성된 내용</param>
        ///<returns></returns>
        /// </summary>
        public void SetQnAWriteOk(QnABoard model)
        {
            SqlConnection db = _connectionContext.Database.GetDbConnection() as SqlConnection;
            db.Open();

            using (SqlCommand cmd = db.CreateCommand())
            {
                string SQL = @"
                    INSERT INTO QnA(Idx, BRef, BLevel, BStep, BTitle, UserName, UserPwd, BContent, HitCount, RegDate, DelDiv)
                    SELECT
	                    (SELECT ISNULL(MAX(Idx), 0) + 1 FROM QnA),--Idx
	                    (SELECT ISNULL(MAX(Idx), 0) + 1 FROM QnA),--BRef
	                    0,--BLevel
	                    0,--BStep
	                    @BTitle,
                        @UserName,
                        @UserPwd,
	                    @BContent,
	                    0,--HitCount
	                    GETDATE(),--RegDate
	                    0--DelDiv
                ";
                cmd.CommandText = SQL;
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@BTitle", model.BTitle);
                cmd.Parameters.AddWithValue("@UserName", model.UserName);
                cmd.Parameters.AddWithValue("@UserPwd", model.UserPwd);
                cmd.Parameters.AddWithValue("@BContent", model.BContent);
                cmd.ExecuteNonQuery();
            }

            db.Close();
        }
        #endregion

        #region 조회수 증가 - SetQnAHitCount()
        ///<summary>
        ///Q&A 글 확인 시 조회수를 하나 증가한다.
        ///<param name="idx">게시글 PK</param>
        ///<returns></returns>
        /// </summary>
        public void SetQnAHitCount(int idx)
        {
            SqlConnection db = _connectionContext.Database.GetDbConnection() as SqlConnection;
            db.Open();

            using (SqlCommand cmd = db.CreateCommand())
            {
                string SQL = @"
                    UPDATE
	                    QnA
                    SET
	                    HitCount = HitCount + 1
                    WHERE
	                    DelDiv = 0
	                    AND Idx = @Idx
                ";
                cmd.CommandText = SQL;
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@Idx", idx);

                cmd.ExecuteNonQuery();
            }

            db.Close();
        }
        #endregion

        #region 글 보기 - GetQnAView()
        ///<summary>
        ///Q&A 특정 게시글 확인
        ///<param name="idx">게시글 PK</param>
        ///<returns>idx에 따른 제목, 등록자, 내용 등 보여줌</returns>
        /// </summary>
        public DataTable GetQnAView(int idx)//묻고답하기 보기
        {
            SqlConnection db = _connectionContext.Database.GetDbConnection() as SqlConnection;
            db.Open();

            DataTable dt = new DataTable();

            using (SqlCommand cmd = db.CreateCommand())
            {
                string SQL = @"
                    SELECT
	                    BRef,
	                    BLevel,
	                    BStep,
	                    BTitle,
	                    UserName,
	                    UserPwd,
	                    BContent,
	                    HitCount,
	                    RegDate
                    FROM
	                    QnA
                    WHERE
	                    DelDiv = 0
	                    AND Idx = @idx
                ";
                cmd.CommandText = SQL;
                cmd.CommandType = System.Data.CommandType.Text;
                cmd.Parameters.AddWithValue("@idx", idx);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);
            }

            db.Close();

            return dt;
        }
        #endregion

        #region 글 답변 - GetQnAReplyOk()
        ///<summary>
        ///게시글에 대해 답변하기
        ///<param name="model">model</param>
        ///<param name="idx">게시글 PK</param>
        ///<param name="bRef"></param>
        ///<param name="bLevel"></param>
        ///<param name="bStep"></param>
        ///<returns></returns>
        /// </summary>
        public void GetQnAReplyOk(QnABoard model, int idx, int bRef, int bLevel, int bStep)
        {
            SqlConnection db = _connectionContext.Database.GetDbConnection() as SqlConnection;
            db.Open();

            using (SqlCommand cmd1 = db.CreateCommand())
            {
                string SQL = @"
                    UPDATE
	                    QnA
                    SET
	                    BStep = BStep + 1
                    WHERE
	                    DelDiv = 0
	                    AND BRef = @BRef
	                    AND BStep > @BStep
                ";
                cmd1.CommandText = SQL;
                cmd1.CommandType = CommandType.Text;
                cmd1.Parameters.AddWithValue("@BRef", bRef);
                cmd1.Parameters.AddWithValue("@BStep", bStep);
                cmd1.ExecuteNonQuery();
            }

            using (SqlCommand cmd2 = db.CreateCommand())
            {
                string SQL = @"
                    INSERT INTO QnA(Idx, BRef, BLevel, BStep, BTitle, UserName, UserPwd, BContent, HitCount, RegDate, DelDiv)
                    SELECT
	                    (SELECT ISNULL(MAX(Idx), 0) + 1 FROM QnA),--IDX
	                    @BRef,
	                    @BLevel,
	                    @BStep,
	                    @BTitle,
                        @UserName,
                        @UserPwd,
	                    @BContent,
	                    0,--HitCount
	                    GETDATE(),--RegDate
	                    0--DelDiv
                ";
                cmd2.CommandText = SQL;
                cmd2.CommandType = CommandType.Text;
                cmd2.Parameters.AddWithValue("@BRef", bRef);
                cmd2.Parameters.AddWithValue("@BLevel", Convert.ToInt32(bLevel) + 1);
                cmd2.Parameters.AddWithValue("@BStep", Convert.ToInt32(bStep) + 1);
                cmd2.Parameters.AddWithValue("@BTitle", model.BTitle);
                cmd2.Parameters.AddWithValue("@UserName", model.UserName);
                cmd2.Parameters.AddWithValue("@UserPwd", model.UserPwd);
                cmd2.Parameters.AddWithValue("@BContent", model.BContent);
                cmd2.ExecuteNonQuery();
            }

            db.Close();
        }
        #endregion

        #region 비밀번호 비교 - QnAPwdCompare()
        ///<summary>
        ///게시글에 대해 답변하기
        ///<param name="idx">게시글 PK</param>
        ///<returns>DB에 들어있는 idx의 비밀번호</returns>
        /// </summary>
        public QnABoard QnAPwdCompare(int idx)
        {
            SqlConnection db = _connectionContext.Database.GetDbConnection() as SqlConnection;
            db.Open();

            QnABoard qnaList = new QnABoard();

            using (SqlCommand cmd = db.CreateCommand())
            {
                var strSql = @"
                    SELECT
	                    UserPwd
                    FROM
	                    QnA
                    WHERE
	                    DelDiv = 0
	                    AND Idx = @idx
				";
                cmd.CommandText = strSql;
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@idx", idx);

                SqlDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    qnaList.UserPwd = dr["UserPwd"].ToString();
                }

                dr.Close();
            }

            db.Close();

            return qnaList;
        }
        #endregion

        #region 글 수정 - SetQnAModifyOk()
        ///<summary>
        ///비밀번호가 맞을 경우 게시글 수정하기
        ///<param name="model"></param>
        ///<param name="idx">게시글 PK</param>
        ///<returns></returns>
        /// </summary>
        public void SetQnAModifyOk(QnABoard model, int idx)
        {
            SqlConnection db = _connectionContext.Database.GetDbConnection() as SqlConnection;
            db.Open();

            using (SqlCommand cmd = db.CreateCommand())
            {
                string SQL = @"
                    UPDATE
	                    QnA
                    SET
	                    BTitle = @BTitle,
	                    BContent = @BContent,
	                    UserName = @UserName
                    WHERE
	                    DelDiv = 0
	                    AND Idx = @idx
                ";
                cmd.CommandText = SQL;
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@BTitle", model.BTitle);
                cmd.Parameters.AddWithValue("@BContent", model.BContent);
                cmd.Parameters.AddWithValue("@UserName", model.UserName);
                cmd.Parameters.AddWithValue("@idx", idx);

                cmd.ExecuteNonQuery();
            }

            db.Close();
        }
        #endregion

        #region 글 삭제 - SetQnADeleteOk()
        ///<summary>
        ///비밀번호가 맞을 경우 게시글 수정하기
        ///<param name="idx">게시글 PK</param>
        ///<param name="BRef"></param>
        ///<param name="BLevel"></param>
        ///<param name="BStep"></param>
        ///<returns></returns>
        /// </summary>
        public void SetQnADeleteOk(int idx, int bRef, int bLevel, int bStep)
        {
            SqlConnection db = _connectionContext.Database.GetDbConnection() as SqlConnection;
            db.Open();

            using (SqlCommand cmd = db.CreateCommand())
            {
                string SQL = @"
                    UPDATE
	                    QnA
                    SET
	                    DelDiv = 1
                    WHERE
                ";
                if (Convert.ToInt32(bStep) == 0)
                {
                    SQL += @"
                        BRef = @BRef
                    ";
                }
                else
                {
                    SQL += @"
                        Idx = @idx
                    ";
                }
                cmd.CommandText = SQL;
                cmd.CommandType = CommandType.Text;
                if (Convert.ToInt32(bStep) == 0)
                {
                    cmd.Parameters.AddWithValue("@BRef", Convert.ToInt32(bRef));
                }
                else
                {
                    cmd.Parameters.AddWithValue("@idx", Convert.ToInt32(idx));
                }

                cmd.ExecuteNonQuery();
            }

            db.Close();
        }
        #endregion
    }
}
