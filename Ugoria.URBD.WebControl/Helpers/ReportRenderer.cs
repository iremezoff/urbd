using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MvcContrib.UI.Grid;
using Ugoria.URBD.WebControl.ViewModels;

namespace Ugoria.URBD.WebControl.Helpers
{
    public class ReportRenderer : HtmlTableGridRenderer<ReportViewModel>
    {
        protected override bool ShouldRenderHeader()
        {
            return true;
        }
    }

    public class CustomRenderer<T> : HtmlTableGridRenderer<T> where T : class
    {
        protected override bool ShouldRenderHeader()
        {
            return true;
        }
    }

    public class LogRenderer : HtmlTableGridRenderer<ServiceLogViewModel>
    {
        protected override bool ShouldRenderHeader()
        {
            return true;
        }
    }

    public static class EditableRenderer
    {
        public static IGridColumn<T> Field<T>(this IGridColumn<T> gridColumn, RenderField renderField) where T:class
        {
            return gridColumn.Attributes(field => renderField);
        }
    }

    public enum FieldType { text, time, radio, list }

    public class RenderField
    {
        private string name;

        public string Name
        {
            get { return name; }
            set { name = value; }
        }
        private FieldType fieldType;

        public FieldType FieldType
        {
            get { return fieldType; }
            set { fieldType = value; }
        }
        private string defaultValue;

        public string DefaultValue
        {
            get { return defaultValue; }
            set { defaultValue = value; }
        }
    }
}