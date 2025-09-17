﻿using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ProberViewModel.ViewModel.ResultMap
{
    class JsonHeaderLogic
    {
        //用于对应Json对象类型的格式化字符
        const string NULL_TEXT = "<null>";
        const string ARRAY = "[{0}]";
        const string OBJECT = "[{0}]";
        const string PROPERTY = "{0}";

        //用于界面绑定的属性定义
        public string Header { get; private set; }
        public IEnumerable<JsonHeaderLogic> Children { get; private set; }
        public JToken Token { get; private set; }

        //内部构造函数，使用FromJToken来创建JsonHeaderLogic
        JsonHeaderLogic(JToken token, string header, IEnumerable<JsonHeaderLogic> children)
        {
            Token = token;
            Header = header;
            Children = children;
        }

        //外部的从JToken创建JsonHeaderLogic的方法
        public static JsonHeaderLogic FromJToken(JToken jtoken)
        {
            if (jtoken == null)
            {
                throw new ArgumentNullException("jtoken");
            }

            var type = jtoken.GetType();

            if (typeof(JValue).IsAssignableFrom(type))
            {
                var jvalue = (JValue)jtoken;
                var value = jvalue.Value;
                if (value == null)
                    value = NULL_TEXT;
                return new JsonHeaderLogic(jvalue, value.ToString(), null);
            }
            else if (typeof(JContainer).IsAssignableFrom(type))
            {
                var jcontainer = (JContainer)jtoken;
                var children = jcontainer.Children().Select(c => FromJToken(c));
                string header;

                if (typeof(JProperty).IsAssignableFrom(type))
                    header = String.Format(PROPERTY, ((JProperty)jcontainer).Name);
                else if (typeof(JArray).IsAssignableFrom(type))
                    header = String.Format(ARRAY, children.Count());
                else if (typeof(JObject).IsAssignableFrom(type))
                    header = String.Format(OBJECT, children.Count());
                else
                    throw new Exception("不支持的JContainer类型");

                return new JsonHeaderLogic(jcontainer, header, children);
            }
            else
            {
                throw new Exception("不支持的JToken类型");
            }
        }
    }
}
