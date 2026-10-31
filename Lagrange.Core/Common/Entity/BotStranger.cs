namespace Lagrange.Core.Common.Entity;

public class BotStranger(long uin, string nickname, string uid, string personalSign, string remark, ulong level, BotGender gender, long registrationTime, long? birthday, ulong age, string qID, string country, string city, string? school) : BotContact
{
    public override long Uin { get; } = uin;

    public override string Nickname { get; } = nickname ?? string.Empty;

    public override string Uid { get; } = uid ?? string.Empty;

    public string PersonalSign { get; } = personalSign;

    public string Remark { get; internal set; } = remark;

    public ulong Level { get; } = level;

    public BotGender Gender { get; } = gender;

    public long RegistrationTime { get; } = registrationTime;

    public long? Birthday { get; } = birthday;

    public int? BirthdayYear { get; init; }
    public int? BirthdayMonth { get; init; }
    public int? BirthdayDay { get; init; }
    public bool? Vip { get; init; }
    public bool? YearVip { get; init; }
    public bool? SuperVip { get; init; }
    public uint? VipLevel { get; init; }

    public ulong Age { get; } = age;

    public string QID { get; } = qID;

    public long Source { get; init; }

    public string Country { get; init; } = country;

    public string City { get; init; } = city;

    public string? School { get; init; } = school;

    internal BotStranger CloneWithSource(long source) => new(
        Uin,
        Nickname,
        Uid,
        PersonalSign,
        Remark,
        Level,
        Gender,
        RegistrationTime,
        Birthday,
        Age,
        QID,
        Country,
        City,
        School
    )
    { Source = source, BirthdayYear = BirthdayYear, BirthdayMonth = BirthdayMonth, BirthdayDay = BirthdayDay, Vip = Vip, YearVip = YearVip, SuperVip = SuperVip, VipLevel = VipLevel };
}
