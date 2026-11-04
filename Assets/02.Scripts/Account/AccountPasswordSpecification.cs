using System.Text.RegularExpressions;

public class AccountPasswordSpecification
{
    // 8~30글자 + 대문자/소문자/숫자/기호를 각각 하나 이상 포함
    // (?=.*[A-Z])        : 어딘가에 대문자가 있어야 한다(앞을 내다보는 조건, 글자를 소비하지 않음).
    // (?=.*[a-z])        : 소문자가 있어야 한다.
    // (?=.*[0-9])        : 숫자가 있어야 한다.
    // (?=.*[^A-Za-z0-9]) : 영문·숫자가 아닌 글자(기호)가 있어야 한다.
    // [!-~]{8,30}        : 화면에 보이는 ASCII 글자(공백, 한글 제외)만 8~30글자.
    private static readonly Regex PasswordRegex = new Regex(@"^(?=.*[A-Z])(?=.*[a-z])(?=.*[0-9])(?=.*[^A-Za-z0-9])[!-~]{8,30}$");

    public string ErrorMessage => "비밀번호는 대문자, 소문자, 숫자, 기호를 포함한 8~30글자여야 합니다(공백, 한글 제외).";

    public bool IsSatisfiedBy(string password)
    {
        return PasswordRegex.IsMatch(password);
    }
}
