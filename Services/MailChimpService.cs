using MailChimp.Net;
using MailChimp.Net.Interfaces;
using MailChimp.Net.Models;
using MailChimp.Net.Core;

namespace KLDShop.Services
{
    public class MailChimpService
    {
        private readonly IMailChimpManager _mailChimpManager;
        private readonly string _listId;
        private readonly IConfiguration _configuration;
        private readonly ILogger<MailChimpService> _logger;

        public MailChimpService(IConfiguration configuration, ILogger<MailChimpService> logger)
        {
            _configuration = configuration;
            _logger = logger;

            var apiKey = _configuration["MailChimp:ApiKey"];
            _listId = _configuration["MailChimp:ListId"] ?? "";

            if (!string.IsNullOrEmpty(apiKey))
            {
                _mailChimpManager = new MailChimpManager(apiKey);
            }
            else
            {
                throw new ArgumentException("MailChimp API Key is not configured");
            }
        }

        /// <summary>
        /// Subscribe a member to the mailing list
        /// </summary>
        public async Task<bool> SubscribeAsync(string email, string? firstName = null, string? lastName = null)
        {
            try
            {
                var member = new Member
                {
                    EmailAddress = email,
                    StatusIfNew = Status.Subscribed,
                    Status = Status.Subscribed
                };

                if (!string.IsNullOrEmpty(firstName) || !string.IsNullOrEmpty(lastName))
                {
                    member.MergeFields = new Dictionary<string, object>();
                    if (!string.IsNullOrEmpty(firstName))
                        member.MergeFields.Add("FNAME", firstName);
                    if (!string.IsNullOrEmpty(lastName))
                        member.MergeFields.Add("LNAME", lastName);
                }

                var result = await _mailChimpManager.Members.AddOrUpdateAsync(_listId, member);
                _logger.LogInformation($"Successfully subscribed {email} to MailChimp list");
                return result != null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error subscribing {email} to MailChimp");
                return false;
            }
        }

        /// <summary>
        /// Unsubscribe a member from the mailing list
        /// </summary>
        public async Task<bool> UnsubscribeAsync(string email)
        {
            try
            {
                var member = new Member
                {
                    EmailAddress = email,
                    Status = Status.Unsubscribed
                };

                await _mailChimpManager.Members.AddOrUpdateAsync(_listId, member);
                _logger.LogInformation($"Successfully unsubscribed {email} from MailChimp list");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error unsubscribing {email} from MailChimp");
                return false;
            }
        }

        /// <summary>
        /// Create an email campaign
        /// </summary>
        public async Task<Campaign?> CreateCampaignAsync(string subject, string fromName, string fromEmail, string replyTo, string htmlContent, string? previewText = null)
        {
            try
            {
                var campaign = new Campaign
                {
                    Type = CampaignType.Regular,
                    Recipients = new Recipient
                    {
                        ListId = _listId
                    },
                    Settings = new Setting
                    {
                        SubjectLine = subject,
                        PreviewText = previewText ?? subject,
                        FromName = fromName,
                        ReplyTo = replyTo,
                        Title = $"{subject} - {DateTime.UtcNow:yyyy-MM-dd HH:mm}"
                    }
                };

                var createdCampaign = await _mailChimpManager.Campaigns.AddAsync(campaign);

                // Set campaign content
                var contentRequest = new ContentRequest
                {
                    Html = htmlContent
                };
                await _mailChimpManager.Content.AddOrUpdateAsync(createdCampaign.Id, contentRequest);

                _logger.LogInformation($"Successfully created campaign: {createdCampaign.Id}");
                return createdCampaign;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating MailChimp campaign");
                return null;
            }
        }

        /// <summary>
        /// Send a campaign
        /// </summary>
        public async Task<bool> SendCampaignAsync(string campaignId)
        {
            try
            {
                await _mailChimpManager.Campaigns.SendAsync(campaignId);
                _logger.LogInformation($"Successfully sent campaign: {campaignId}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending campaign {campaignId}");
                return false;
            }
        }

        /// <summary>
        /// Schedule a campaign to be sent at a specific time
        /// </summary>
        public async Task<bool> ScheduleCampaignAsync(string campaignId, DateTime scheduleTime)
        {
            try
            {
                var scheduleRequest = new CampaignScheduleRequest
                {
                    ScheduleTime = scheduleTime.ToString("yyyy-MM-ddTHH:mm:sszzz")
                };
                await _mailChimpManager.Campaigns.ScheduleAsync(campaignId, scheduleRequest);
                _logger.LogInformation($"Successfully scheduled campaign {campaignId} for {scheduleTime}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error scheduling campaign {campaignId}");
                return false;
            }
        }

        /// <summary>
        /// Get campaign statistics
        /// </summary>
        public async Task<Report?> GetCampaignReportAsync(string campaignId)
        {
            try
            {
                var report = await _mailChimpManager.Reports.GetReportAsync(campaignId);
                return report;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting report for campaign {campaignId}");
                return null;
            }
        }

        /// <summary>
        /// Get all campaigns
        /// </summary>
        public async Task<IEnumerable<Campaign>> GetCampaignsAsync(int count = 50)
        {
            try
            {
                var campaigns = await _mailChimpManager.Campaigns.GetAllAsync();
                return campaigns.Take(count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting campaigns from MailChimp");
                return Enumerable.Empty<Campaign>();
            }
        }

        /// <summary>
        /// Get subscriber count
        /// </summary>
        public async Task<int> GetSubscriberCountAsync()
        {
            try
            {
                var list = await _mailChimpManager.Lists.GetAsync(_listId);
                return list?.Stats?.MemberCount ?? 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting subscriber count from MailChimp");
                return 0;
            }
        }

        /// <summary>
        /// Check if email is subscribed
        /// </summary>
        public async Task<bool> IsSubscribedAsync(string email)
        {
            try
            {
                var member = await _mailChimpManager.Members.GetAsync(_listId, email);
                return member?.Status == Status.Subscribed;
            }
            catch
            {
                return false;
            }
        }
    }
}
