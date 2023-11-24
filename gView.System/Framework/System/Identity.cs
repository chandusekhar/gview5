using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace gView.Framework.system
{
    public class Identity : IIdentity
    {
        private string _name;
        private List<string> _roles;
        private string _hashedPassword = String.Empty;

        public Identity(string username, bool isAdministrator = false)
            : this(username, null, isAdministrator)
        {
        }
        public Identity(string username, List<string> roles, bool isAdministrator = false)
        {
            _name = String.IsNullOrWhiteSpace(username) ? AnonyomousUsername : username;
            _roles = (roles != null) ? roles : new List<string>();

            this.IsAdministrator = isAdministrator;
        }

        public string UserName
        {
            get { return _name; }
        }

        public List<string> UserRoles
        {
            get { return _roles; }
        }

        public string ToFormattedString()
        {
            if (String.IsNullOrEmpty(_name))
            {
                return String.Empty;
            }

            StringBuilder sb = new StringBuilder();
            sb.Append(encodeString(_name));

            if (_roles != null)
            {
                foreach (string role in _roles)
                {
                    if (String.IsNullOrEmpty(role))
                    {
                        continue;
                    }

                    sb.Append("|" + encodeString(role));
                }
            }
            return sb.ToString();
        }

        public bool IsAdministrator { get; private set; }

        public bool IsAnonymous => String.IsNullOrEmpty(_name) || _name == AnonyomousUsername;

        //public string HashedPassword
        //{
        //    get { return _hashedPassword; }
        //    set { _hashedPassword = value; }
        //}

        public static Identity FromFormattedString(string str)
        {
            if (String.IsNullOrEmpty(str))
            {
                return new Identity("");
            }

            string[] parts = str.Split('|');

            string name = decodeString(parts[0]);
            List<string> roles = new List<string>();
            for (int i = 1; i < parts.Length; i++)
            {
                roles.Add(decodeString(parts[i]));
            }

            return new Identity(name, roles);
        }

        public const string AnonyomousUsername = "_anonymous";

        #region Helper
        static private string encodeString(string str)
        {
            return str.Replace("|", "&pipe;");
        }
        static private string decodeString(string str)
        {
            return str.Replace("&pipe;", "|");
        }
        #endregion

        #region PasswordHash
        static public string HashPassword(string password)
        {
            if (String.IsNullOrEmpty(password))
            {
                return String.Empty;
            }

            using (var md5 = MD5.Create())
            {
                byte[] byteValue = UTF8Encoding.UTF8.GetBytes(password);
                byte[] byteHash = md5.ComputeHash(byteValue);
                md5.Clear();

                return Convert.ToBase64String(byteHash);
            }
        }
        #endregion
    }
}
