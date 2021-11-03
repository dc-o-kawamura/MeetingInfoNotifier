using System;
using FluentValidation;

namespace dcinc.api.entities
{
    public class WebMeetingValidator : AbstractValidator<WebMeeting>
    {
        public WebMeetingValidator()
        {
            // 会議名が未指定の場合は無効な値とする。
            RuleFor(webMeeting => webMeeting.Name).NotNull().NotEmpty().WithMessage("Name is null or empty");
            // 日付が未指定もしくは今日以前の場合は無効な値とする。
            RuleFor(webMeeting => webMeeting.StartDateTime).NotNull().NotEmpty().When(webMeeting => webMeeting.StartDateTime <= DateTime.Today).WithMessage("StartDateTime is invalid. Please specify the date and time after tomorrow.");
            // SlackチャンネルIDが未指定の場合は無効な値とする。
            RuleFor(webMeeting => webMeeting.SlackChannelId).NotNull().NotEmpty().WithMessage("SlackChannelId is null or empty");
        }
    }
}
