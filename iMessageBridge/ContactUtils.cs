using MonoMac.Foundation;
using System;
using System.Runtime.InteropServices;

namespace DylanBriedis.iMessageBridge
{
    internal static class ContactUtils
    {
        [DllImport("libContactUtils.dylib")]
        static extern IntPtr GetPersonFromNumber(string number);

        [DllImport("libContactUtils.dylib")]
        static extern IntPtr GetPersonFromEmail(string email);

        [DllImport("libContactUtils.dylib")]
        static extern IntPtr GetNameFromPerson(IntPtr person);

        [DllImport("libContactUtils.dylib")]
        static extern IntPtr GetPictureFromPerson(IntPtr person);

        [DllImport("libContactUtils.dylib")]
        static extern uint GetPictureLengthFromPerson(IntPtr person);

        static string[] countryCodes = new string[] { "+93", "+355", "+213", "+684", "+376", "+244", "+1-264", "+672", "+1-268", "+54", "+374", "+297", "+61", "+43", "+994", "+1-242", "+973", "+880", "+1-246", "+375", "+32", "+501", "+229", "+1-441", "+975", "+591", "+387", "+267", "+55", "+673", "+359", "+226", "+257", "+855", "+237", "+1", "+238", "+1-345", "+236", "+235", "+56", "+86", "+61", "+61", "+57", "+269", "+242", "+243", "+682", "+506", "+385", "+53", "+357", "+420", "+45", "+253", "+1-767", "+809", "+593", "+20", "+503", "+240", "+291", "+372", "+251", "+500", "+298", "+679", "+358", "+33", "+594", "+241", "+220", "+995", "+49", "+233", "+350", "+44", "+30", "+299", "+1-473", "+590", "+1-671", "+502", "+224", "+245", "+592", "+509", "+504", "+852", "+36", "+354", "+91", "+62", "+98", "+964", "+353", "+972", "+39", "+225", "+1-876", "+81", "+962", "+7", "+254", "+686", "+850", "+82", "+965", "+996", "+856", "+371", "+961", "+266", "+231", "+218", "+423", "+370", "+352", "+853", "+389", "+261", "+265", "+60", "+960", "+223", "+356", "+692", "+596", "+222", "+230", "+269", "+52", "+691", "+373", "+377", "+976", "+382", "+1-664", "+212", "+258", "+95", "+264", "+674", "+977", "+31", "+599", "+687", "+64", "+505", "+227", "+234", "+683", "+672", "+670", "+47", "+968", "+92", "+680", "+507", "+675", "+595", "+51", "+63", "+48", "+689", "+351", "+1-787", "+974", "+262", "+40", "+7", "+250", "+290", "+1-869", "+1-758", "+508", "+1-784", "+684", "+378", "+239", "+966", "+221", "+381", "+248", "+232", "+65", "+421", "+386", "+677", "+252", "+27", "+34", "+94", "+249", "+597", "+268", "+46", "+41", "+963", "+886", "+992", "+255", "+66", "+228", "+690", "+676", "+1-868", "+216", "+90", "+993", "+1-649", "+688", "+44", "+256", "+380", "+971", "+598", "+1", "+998", "+678", "+39", "+58", "+84", "+1-284", "+1-340", "+681", "+967", "+260", "+263" };

        public static Person GetPerson(string numberOrEmail)
        {
            IntPtr person;
            string formattedPhoneNumber = numberOrEmail;
            foreach (string code in countryCodes)
                // Remove any country codes from the phone number so it can detect correctly.
                formattedPhoneNumber = formattedPhoneNumber.Replace(code, "");
            person = GetPersonFromNumber(formattedPhoneNumber);
            if (person == IntPtr.Zero)
            {
                person = GetPersonFromEmail(numberOrEmail);
                if (person == IntPtr.Zero)
                    return new Person() { name = numberOrEmail, picture = null };
            }
            Person result = new Person();
            result.name = NSString.FromHandle(GetNameFromPerson(person));
            if (string.IsNullOrEmpty(result.name.Trim())) // We don't need any blank names!
                result.name = numberOrEmail;
            IntPtr pictureSrc = GetPictureFromPerson(person);
            if (pictureSrc != IntPtr.Zero)
            {
                uint pictureLength = GetPictureLengthFromPerson(person);
                byte[] picture = new byte[pictureLength];
                Marshal.Copy(pictureSrc, picture, 0, (int)pictureLength);
                result.picture = picture;
            }
            return result;
        }
    }

    internal class Person
    {
        public string name;
        public byte[] picture;
    }
}
