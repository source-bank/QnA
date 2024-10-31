using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QnA.Include;
using QnA.Models;
using QnA.Models.Tables;
using System;
using System.Data;

namespace QnA.Controllers
{
    public class QnABoardController : Controller
    {
        //DB작업을 위해
        private readonly DBConnectionContext _context;

        //paging 변수 start
        public int PageIndex { get; set; } = 0;
        public bool SearchMode { get; set; } = false;
        public string SearchCondition { get; set; } = "";
        public string SearchKeyword { get; set; } = "";
        public int TotalRecordCount { get; set; } = 0;
        //paging 변수 end

        public QnABoardController(DBConnectionContext context)
        {
            _context = context;
        }

        // 목록
        public IActionResult Index() //기본 ActionResult를 IActionResult로 변경함(여러 결과의 타입을 포함, 다양한 형태의 결과를 반환, 더 유연)
        {
            //검색 모드인지 아닌지 확인
            SearchMode = (!string.IsNullOrEmpty(Request.Query["SearchCondition"].ToString()) && !string.IsNullOrEmpty(Request.Query["SearchKeyword"].ToString()));
            if (SearchMode)//검색이라면 넘어온 변수의 값들을 저장
            {
                SearchCondition = Request.Query["SearchCondition"].ToString();
                SearchKeyword = Request.Query["SearchKeyword"].ToString();
            }

            //페이지 번호 유무 확인
            if (!string.IsNullOrEmpty(Request.Query["CurrentPage"].ToString()))
            {
                PageIndex = Convert.ToInt32(Request.Query["CurrentPage"].ToString()) - 1;//인덱스가 0번 부터라...
            }


            //데이터를 가지고 와볼까..
            DAL dal = new DAL(_context);// Models/DAL.cs에 있지

            //전체 게시글 수
            TotalRecordCount = dal.GetQnACount(SearchCondition, SearchKeyword);

            //전체 글
            DataTable dt = dal.GetQnAList(PageIndex, SearchCondition, SearchKeyword);
            if (dt.Rows.Count == 0)
            {
                ViewBag.DataList = null;
            }
            else
            {
                ViewBag.DataList = dt;
            }

            //기타 페이징 관련된 것도 ViewBag에 저장해서 목록페이지에서 사용하자
            ViewBag.TotalRecordCount = TotalRecordCount;
            ViewBag.SearchMode = SearchMode;
            ViewBag.SearchCondition = SearchCondition;
            ViewBag.SearchKeyword = SearchKeyword;

            return View();
        }

        // 보기
        [HttpGet]
        public IActionResult Details(int idx, int currentPage, string searchCondition, string searchKeyword)
        {
            DAL dal = new DAL(_context);

            //조회수 증가
            dal.SetQnAHitCount(idx);

            //데이터 가져오기
            var qnaTbl = dal.GetQnAView(idx);
            ViewBag.Details = qnaTbl;

            return View();
        }

        // 등록
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        //등록 처리
        [HttpPost]
        public IActionResult Create(QnABoard model)
        { 
            DAL dal = new DAL(_context);

            dal.SetQnAWriteOk(model);

            return RedirectToAction("Index", "QnABoard");
        }

        // 답변
        [HttpGet]
        public IActionResult Reply(int idx, int currentPage, string searchCondition, string searchKeyword, int bRef, int bLevel, int bStep)
        { 
            DAL dal = new DAL(_context);

            //데이터 가져오기
            var qnaTbl = dal.GetQnAView(idx);

            string bTitle = qnaTbl.Rows[0]["BTitle"].ToString();
            string bContent = qnaTbl.Rows[0]["BContent"].ToString();

            ViewBag.Title = bTitle;
            ViewBag.Content = bContent;

            //각종 파라미터
            ViewBag.Idx = idx;
            ViewBag.CurrentPage = currentPage;
            ViewBag.SearchCondition = searchCondition;
            ViewBag.SearchKeyword = searchKeyword;
            ViewBag.BRef = bRef;
            ViewBag.BLevel = bLevel;
            ViewBag.BStep = bStep;

            return View();
        }

        // 답변 처리
        [HttpPost]
        public IActionResult Reply(QnABoard model, int idx, int currentPage, string searchCondition, string searchKeyword, int bRef, int bLevel, int bStep)
        {
            DAL dal = new DAL(_context);

            dal.GetQnAReplyOk(model, idx, bRef, bLevel, bStep);

            return RedirectToAction("Index", "QnABoard", new { CurrentPage = currentPage, SearchCondition = searchCondition, SearchKeyword = searchKeyword });
        }

        // 수정
        [HttpGet]
        public IActionResult Edit(int idx, int currentPage, string searchCondition, string searchKeyword)
        {
            DAL dal = new DAL(_context);

            //데이터 가져오기
            var qnaTbl = dal.GetQnAView(idx);
            ViewBag.Details = qnaTbl;

            //각종 파라미터
            ViewBag.Idx = idx;
            ViewBag.CurrentPage = currentPage;
            ViewBag.SearchCondition = searchCondition;
            ViewBag.SearchKeyword = searchKeyword;

            return View();
        }

        // 수정 처리
        [HttpPost]
        public IActionResult Edit(QnABoard model, int idx, int currentPage, string searchCondition, string searchKeyword)
        {
            DAL dal = new DAL(_context);

            //원 글의 비밀번호 확인
            var orginPwd = dal.QnAPwdCompare(idx);

            if (model.UserPwd.ToString() == orginPwd.UserPwd.ToString())
            {
                //업데이트
                dal.SetQnAModifyOk(model, idx);

                return RedirectToAction("Index", "QnABoard", new { CurrentPage = currentPage, SearchCondition = searchCondition, SearchKeyword = searchKeyword });
            }
            else
            {
                string alert = "등록된 비밀번호가 일치하지 않습니다.";
                string script = "<script>alert(" + System.Text.Json.JsonSerializer.Serialize(alert) + "); location.href='/QnABoard/Edit?idx=" + idx + "&CurrentPage=" + currentPage + "&SearchCondition=" + searchCondition + "&SearchKeyword=" + searchKeyword + "'</script>";
                return Content(script, "text/html");
            }
        }

        // 삭제
        [HttpGet]
        public IActionResult Delete(int idx, int currentPage, string searchCondition, string searchKeyword, int bRef, int bLevel, int bStep)
        {
            //각종 파라미터
            ViewBag.Idx = idx;
            ViewBag.CurrentPage = currentPage;
            ViewBag.SearchCondition = searchCondition;
            ViewBag.SearchKeyword = searchKeyword;
            ViewBag.BRef = bRef;
            ViewBag.BLevel = bLevel;
            ViewBag.BStep = bStep;

            return View();
        }

        // 삭제 처리
        [HttpPost]
        public IActionResult Delete(QnABoard model, int idx, int currentPage, string searchCondition, string searchKeyword, int bRef, int bLevel, int bStep)
        {
            DAL dal = new DAL(_context);

            //원 글의 비밀번호 확인
            var orginPwd = dal.QnAPwdCompare(idx);

            if (model.UserPwd.ToString() == orginPwd.UserPwd.ToString())
            {
                //글 삭제
                dal.SetQnADeleteOk(idx, bRef, bLevel, bStep);

                return RedirectToAction("Index", "QnABoard", new { CurrentPage = currentPage, SearchCondition = searchCondition, SearchKeyword = searchKeyword });
            }
            else
            {
                string alert = "등록된 비밀번호가 일치하지 않습니다.";
                string script = "<script>alert(" + System.Text.Json.JsonSerializer.Serialize(alert) + "); location.href='/QnABoard/Delete?idx=" + idx + "&CurrentPage=" + currentPage + "&SearchCondition=" + searchCondition + "&SearchKeyword=" + searchKeyword + "'</script>";
                return Content(script, "text/html");
            }
        }
    }
}
