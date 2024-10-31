using Microsoft.AspNetCore.Razor.TagHelpers;

namespace QnA.Include
{
    [HtmlTargetElement("CommonPager")]//cshtml 페이지에서 CommonPager 함수로 쓰겠다..고 선언	
    public class PagingHelperTagHelper : TagHelper
    {
        //검색어가 있는 경우 true, 검색어가 없는 경우 false
        public bool SearchMode { get; set; } = false;

        //검색 조건
        public string? SearchCondition { get; set; }

        //검색어
        public string? SearchKeyword { get; set; }

        //페이지 인덱스 1 | 2 | 3...(C#에서는 0번부터 시작됨)
        public int PageIndex { get; set; } = 0;

        //총 페이지 수
        public int TotalPageCount { get; set; }

        //한 페이지에 보여질 게시글 수
        public int PageSize { get; set; } = 10;

        //페이징이 적용될 URL
        public string? Url { get; set; }

        //기타 parameter를 이용하기 위한 변수
        public string? SearchParam { get; set; }


        //총 레코드 수를 구하고 총 레코드 수를 이용해 총 페이지 수를 구함
        private int _RecordCount;
        public int RecordCount
        {
            get { return _RecordCount; }
            set
            {
                _RecordCount = value;
                TotalPageCount = ((_RecordCount - 1) / PageSize) + 1;
            }
        }


        //실제 페이징 (이전 | 1 | 2 | 3 | 다음)
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "ul";
            output.Attributes.Add("class", "pagination justify-content-center");

            if (PageIndex == 0)
            {
                PageIndex = 1;
            }

            int i = 0;
            string strPage = "";

            if (PageIndex > 10)
            {
                strPage += "<li class='page-item'><a class='page-link' href=\"" + Url + "?CurrentPage=" + Convert.ToString(((PageIndex - 1) / (int)10) * 10) + "&SearchCondition=" + SearchCondition + "&SearchKeyword=" + SearchKeyword + "&" + SearchParam + "\"><span>&laquo;</span></a></li>";
            }

            else
            {
                strPage += "<li class='page-item'><a class='page-link'><span>&laquo;</span></a></li>";
            }


            for (i = (((PageIndex - 1) / (int)10) * 10 + 1); i <= ((((PageIndex - 1) / (int)10) + 1) * 10); i++)
            {
                if (i > TotalPageCount)
                {
                    break;
                }

                if (i == PageIndex)
                {
                    strPage += "<li class='page-item active'><a class='page-link' href='#'>" + i.ToString() + "</a></li>";
                }
                else
                {
                    strPage += "<li class='page-item'><a class='page-link' href=\"" + Url + "?CurrentPage=" + i.ToString() + "&SearchCondition=" + SearchCondition + "&SearchKeyword=" + SearchKeyword + "&" + SearchParam + "\">" + i.ToString() + "</a></li>";
                }
            }

            if (i < TotalPageCount)
            {
                strPage += "<li class='page-item'><a class='page-link' href=\"" + Url + "?CurrentPage=" + Convert.ToString(((PageIndex - 1) / (int)10) * 10 + 11) + "&SearchCondition=" + SearchCondition + "&SearchKeyword=" + SearchKeyword + "&" + SearchParam + "\"><span>&raquo;</span></a></li>";
            }
            else
            {
                strPage += "<li class='page-item'><a class='page-link'><span>&raquo;</span></a></li>";
            }

            output.Content.AppendHtml(strPage);
        }
    }
}
