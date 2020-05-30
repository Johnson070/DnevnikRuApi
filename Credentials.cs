using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiDiaryLibrary
{
    class Credentials
    {
        public string client_id = "1d7bd105-4cd1-4f6c-9ecc-394e400b53bd";
        public string client_secret = "5dcb5237-b5d3-406b-8fee-4441c3a66c99";
        public string username;
        public string password;
        public string scope = "Schools,Relatives,EduGroups,Lessons,marks,EduWorks,Avatar,EducationalInfo,CommonInfo,ContactInfo,FriendsAndRelatives,Files,Wall,Messages";

        public Credentials(string pass, string login, string client_id, string client_secret, string scope)
        {
            this.password = pass;
            this.username = login;
            this.client_id = client_id;
            this.client_secret = client_secret;
            this.scope = scope;
        }

        public Credentials(string pass, string login)
        {
            password = pass;
            username = login;
        }
    }
}
