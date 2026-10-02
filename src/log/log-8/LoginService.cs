public class LoginService
{
    // The 'password' parameter will not be logged because of its name.
    public bool VerifyPassword( string account, string password ) => account == password;

    [return: NotLogged]
    public string GetSaltedHash( string account, string password, [NotLogged] string salt )
        => account + password + salt;
}