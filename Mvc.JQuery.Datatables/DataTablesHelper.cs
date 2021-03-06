﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;

namespace Mvc.JQuery.Datatables
{
    public static class DataTablesHelper
    {

        public static IHtmlString DataTableIncludes(this HtmlHelper helper, bool jqueryUi = false, bool filters = true, bool tableTools = true)
        {
            StringBuilder output = new StringBuilder();
            Action<string> addJs = s => output.AppendLine(@"<script src=""" + UrlHelper.GenerateContentUrl(s, helper.ViewContext.HttpContext) + @""" type=""text/javascript""></script>");
            Action<string> addCss = s => output.AppendLine(@"<link type=""text/css"" href=""" + UrlHelper.GenerateContentUrl(s, helper.ViewContext.HttpContext) + @""" rel=""stylesheet""/>");

            addCss("~/Content/DataTables-1.9.2/media/css/" + (jqueryUi ? ("jquery.dataTables_themeroller.css") : "jquery.dataTables.css"));
            addJs("~/Scripts/DataTables-1.9.2/media/js/jquery.dataTables.js");
            if (filters) addJs("~/Scripts/jquery.dataTables.columnFilter.js");
            if (tableTools)
            {
                addJs("~/Scripts/DataTables-1.9.2/extras/TableTools/media/js/ZeroClipboard.js");
                addJs("~/Scripts/DataTables-1.9.2/extras/TableTools/media/js/TableTools.js");
                addCss("~/Content/DataTables-1.9.2/extras/TableTools/media/css/TableTools.css");
            }
            return helper.Raw(output.ToString());

        }

        public static DataTableVm DataTableVm<TController, TResult>(this HtmlHelper html, string id, Expression<Func<TController, DataTablesResult<TResult>>> exp, params Tuple<string, DataTablesColumn>[] columns)
        {
            if (columns == null || columns.Length == 0)
            {
                columns = BuildColumns<TResult>();
            }

            var mi = exp.MethodInfo();
            var controllerName = typeof(TController).Name;
            controllerName = controllerName.Substring(0, controllerName.LastIndexOf("Controller"));
            var urlHelper = new UrlHelper(html.ViewContext.RequestContext);
            var ajaxUrl = urlHelper.Action(mi.Name, controllerName);
            return new DataTableVm(id, ajaxUrl, columns);
        }

        public static Tuple<string, DataTablesColumn>[] BuildColumns<TResult>()
        {
            var props =
                typeof (TResult).GetProperties()
                    .Where(p => p.GetGetMethod() != null)
                    .Select(p => p);
            var propCount = props.Count();
            var columns = new Tuple<string, DataTablesColumn>[propCount];

            for (int i = 0; i < propCount; i++)
            {
                var prop = props.ElementAt(i);
                var col = new DataTablesColumn {Type = prop.PropertyType, Name = prop.Name, Visible = true};

                var displayNameAttribute = prop.GetCustomAttributes(typeof (DisplayNameAttribute), false);
                col.Title = displayNameAttribute.Length > 0
                                ? ((DisplayNameAttribute) displayNameAttribute[0]).DisplayName
                                : prop.Name;

                var scaffoldColumnAttribute = prop.GetCustomAttributes(typeof (ScaffoldColumnAttribute), false);
                if (scaffoldColumnAttribute.Length > 0)
                    col.Visible = ((ScaffoldColumnAttribute) scaffoldColumnAttribute[0]).Scaffold;

                columns[i] = Tuple.Create(prop.Name, col);
            }

            return columns;
        }

        public static DataTableVm DataTableVm(this HtmlHelper html, string id, string ajaxUrl, params string[] columns)
        {
            return new DataTableVm(id, ajaxUrl, columns.Select(c => Tuple.Create(c, new DataTablesColumn { Type = typeof(string), Name = c, Title = c, Visible = true})));
        }
    }
}