using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace FormziApi.Helper
{
    public static class Constants
    {
        #region enum
        public enum Roles
        {
            SubscriberAdmin = 1,
            OperationAdmin = 2,
            Administrator = 3,
            Supervisor = 4,
            MobileUser = 5
        }

        public enum ElementType
        {
            // Standard
            Formheader = 1,
            Text = 2,
            Number = 3,
            Label = 4,
            TextArea = 5,
            Checkbox = 6,
            Radio = 7,
            Select = 8,
            Date = 9,
            Time = 10,
            Section = 11,
            Page = 12,
            // Advanced
            Email = 13,
            Url = 14,
            Name = 15,
            CaptureImage = 16,
            FileUpload = 17,
            GeoLocation = 18,
            Phone = 19,
            Price = 20,
            Measurment = 21,
            Summary = 22,
            Signature = 23,
            Toggle = 24,
            Happiness = 25,
            Barcode = 26,
            Grid = 27,
            Address = 28,
            CaptureVideo = 29,
            Settings = 30,
            Captcha = 31,
            Audio = 32
        }

        public enum SubmissionAction
        {
            Resolved = 1,
            Transfer = 2,
            NoScope = 3,
            Escalated = 4,
            Closed = 5
        }

        public enum ResourceType
        {
            Label = 1,
            Heading = 2,
            Message = 3,
            Error = 4,
            Form = 5,
            App = 6
        }

        #endregion

        #region Constants

        // Operation setting related keys
        public static string DefaultCurrency = "operation.defaultCurrency";
        public static string BaseLanguage = "operation.baseLanguage";
        public static string EmailLanguage = "operation.emailLanguage";
        public static string FlagName = "operation.flagName";
        public static string OperatingLanguage = "operation.operatingLanguage";

        public static string SubscriberBaseLanguage = "BaseLanguage";

        // Employees related keys
        public static string ProfilePic = "profilePic";
        public static string IdProof = "idProof";

        // Image store related keys
        public static string ProfilePicPath = "Employee_Profile_";
        public static string IdProofPath = "Employee_IdProof_";
        public static string EmployeeFolder = "Employees/";
        public static string ImageFolder = "/Images/";
        // Added by Rinkle on 6-june-2015 (start)
        public static string VideoFolder = "/Videos/";
        public static string AudioFolder = "/Audios/";
        public static string FilesFolder = "/Files/";
        // end
        public static string FormFolder = "Forms/";
        public static string JSONFolder = "/JSON/";
        public static string FormSubmissionFolder = "FormSubmissions/";
        public static string FormSubmissionDataFolder = "/FormSubmissions/";
        public static string FileRoot = "FileRoot";
        public static string FileUrl = "FileUrl";
        public static string JPG = ".jpg";
        public static string PNG = ".png";
        public static string JSON = ".json";
        public static string CaptureImage = "FILEUPLOAD";
        public static string LoggedIn = "LoggedIn";
        public static string LoggedOut = "LoggedOut";
        public static string Language = "operation.baseLanguage";
        public static string ResourceForgotPassword = "ForgotPassword";
        public static string SubscriberFolder = "Subscriber/";
        public static string SMSAuthKey = "SMSAuthKey";
        public static string OTPTemplate = "OTPTemplate";
        public static string EmailSignature = "EmailSignature";
        public static string INNFY_DOMAIN = "http://admin.formzi.com/#/";
        public static int NO_OF_CHARACTER = 25;
        public static string ADMIN_PHONE = "AdminPhone";
        public static string INNFY_SUBSCRIBER_ID = "1";
        public static string IS_SEND_NEW_POST_SMS = "IsSendNewPostSMS";
        public static string SMS_ADVERTISEMENT = "SMSAdv";
        public static string POST_RECEIVED = "SMSPostReceived";
        public static string POST_APPROVED = "SMSPostApproved";
        public static string POST_REJECTED = "SMSPostRejected";
        public static string POST_RESOLVED = "SMSPostResolved";
        public static string APP_AUTH_KEY = "AppAuthKey";
        public static string SMS_ROUTE = "SMSRoute";
        public static string SMS_SENDER_ID = "SMSSenderId";
        public static string SMS_NEW_POST_ADMIN = "SMSNewPostAdmin";
        public static int FORM_APPROVED_ID = 1;
        public static string EMERGENCY_RESPONSE_FORM = "EmergencyReponseForm";
        public static string IWITNESS_BROWSER_KEY = "GoogleMapBrowserKey";
        public static string FORM_COMPONENT_SELECT = "SELECT";
        public static int CATEGORY_ISSUE_DURATION = 0;
        public static int CATEGORY_ISSUE_FORWARD_DURATION = 0;
        public static string DOCUMENTS_FOLDER = "/Documents/";
        public static int NOTIFICATION_REJECT = 0;
        public static int NOTIFICATION_APPROVED = 1;
        public static int NOTIFICATION_RESOLVE = 3;
        public static int NOTIFICATION_NEW_POST_ADDED = 5;
        public static int NOTIFICATION_ASSIGN_TO_EMPLOYEE = 10;
        public static int SUBMISSION_NEW = 0;
        public static int SUBMISSION_APPROVED = 1; // when form submission assign to employee
        public static int SUBMISSION_REJECT = 2;
        public static int SUBMISSION_RESOLVE = 3;
        public static int SUBMISSION_ASSIGN_TO_EMPLOYEE = 10; // it can be use for log table
        public static int EmpDocMaxSize = 1500;
        public static int EmpProfilePicMaxSize = 500;
        public static int FormIconMaxSize = 48;
        public static string IWITNESS_TOKEN = "ahSQdOPy3DYQ6hMOUt3u";
        public static string AMC_CCRS_URL = ConfigurationManager.AppSettings["AMC_CCRS_URL"];
        public static string GOOGLE_MAP_KEY = ConfigurationManager.AppSettings["GoogleMapBrowserKey"];
        public static string GOOGLE_MAP_REVERSE_GEO_CODE_KEY = ConfigurationManager.AppSettings["GoogleReverseGeoCodeMapKey"];
        public static string ERROR_EXCEPTION_MESSAGE = "There is some error."; // "An error occurred. Please try agian later";
        public static string ERROR_NO_DATA_FOUND_MESSAGE = "No data found.";
        public static string I_WITNESS_SECRET_KEY = "21232f297a57a5a743894a0e4a801fc3";
        public static string AUTHORIZATION_ERROR = "Authorization has been denied for this request.";
        public static string VPHS_SUBSCRIBER_ID = "3";
        public static string STUDENT_FOLDER = "Students/";
        public static string ELEMENT_TYPE_FORMHEADER = "FORMHEADER";
        public static string ELEMENT_TYPE_TEXT = "TEXT";
        public static string ELEMENT_TYPE_CAPTUREIMAGE = "";
        public static string ELEMENT_TYPE_CAPTCHA = "";
        public static string ELEMENT_TYPE_CAPTUREVIDEO = "";
        public static string ELEMENT_TYPE_AUDIO = "";
        public static string ELEMENT_TYPE_NUMBER = "NUMBER";
        public static string ELEMENT_TYPE_LABEL = "LABEL";
        public static string ELEMENT_TYPE_TEXTAREA = "TEXTAREA";
        public static string ELEMENT_TYPE_CHECKBOX = "CHECKBOX";
        public static string ELEMENT_TYPE_RADIO = "RADIO";
        public static string ELEMENT_TYPE_SELECT = "SELECT";
        public static string ELEMENT_TYPE_DATE = "DATE";
        public static string ELEMENT_TYPE_TIME = "TIME";
        public static string ELEMENT_TYPE_SECTION = "SECTION";
        public static string ELEMENT_TYPE_PAGE = "PAGE";
        public static string ELEMENT_TYPE_EMAIL = "EMAIL";
        public static string ELEMENT_TYPE_URL = "URL";
        public static string ELEMENT_TYPE_NAME = "NAME";
        public static string ELEMENT_TYPE_ADDRESS = "ADDRESS";
        public static string ELEMENT_TYPE_FILEUPLOAD = "FILEUPLOAD";
        public static string ELEMENT_TYPE_GEOLOCATION = "GEOLOCATION";
        public static string ELEMENT_TYPE_PHONE = "PHONE";
        public static string ELEMENT_TYPE_PRICE = "PRICE";
        public static string ELEMENT_TYPE_MEASUREMENT = "MEASUREMENT";
        public static string ELEMENT_TYPE_SUMMARY = "SUMMARY";
        public static string ELEMENT_TYPE_SIGNATURE = "SIGNATURE";
        public static string ELEMENT_TYPE_TOGGLE = "TOGGLE";
        public static string ELEMENT_TYPE_HAPPINESS = "HAPPINESS";
        public static string ELEMENT_TYPE_BARCODE_QRCODE = "Barcode/QR Code";
        public static string ELEMENT_TYPE_GRID = "Grid";
        public static string ELEMENT_TYPE_SETTINGS = "Settings";
        public static string SECURE_WEB_FORM = "SecureWebForm";
        public static string INNFY_ADMIN_EMAIL = "InnfyAdminEmail";
        public static string INNFY_ADMIN_EMAIL_BCC = "InnfyAdminEmailbcc";
        public static string ERROR_HANDLER_TOEMAIL = "ErrorHandlerToEmail";
        public static string FORMZI_DOMAIN = ConfigurationManager.AppSettings["FormziDomain"];
        public static string IWITNESS_DOMAIN = ConfigurationManager.AppSettings["IWitnessDomain"];

        #endregion

    }
}