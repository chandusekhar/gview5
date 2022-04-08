﻿using System;

namespace gView.Framework.system
{
    public class HtmlLinkAttribute : Attribute
    {
        public HtmlLinkAttribute(string template)
        {
            this.LinkTemplate = template;
        }

        public string LinkTemplate { get; set; }
    }
}
