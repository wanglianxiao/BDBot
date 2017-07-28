using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Connector;

namespace Bot_Application1.Dialogs
{
    [LuisModel("08152b88-1702-4701-8a38-15e7ba95a138", "468d17245e914aa8903ff97f5eb4750d")]
    [Serializable]
    public class RootDialog : LuisDialog<object>
    {

        [LuisIntent("")]
        [LuisIntent("None")]
        public async Task None(IDialogContext context, LuisResult result)
        {
            string message = $"Sorry, I don't understand '{result.Query}'. Type 'help' if you need assistance. ";
            await context.PostAsync(message);
            context.Wait(this.MessageReceived);
        }

        [LuisIntent("greeting")]
        [LuisIntent("help")]
        public async Task WelcomeMessage(IDialogContext context, LuisResult result)
        {
            string message = $"Hi ,您需要什么帮助 ?  \n"
                             + "我可以解决Airwatch或outlook上等收发邮箱的问题，请描述您遇到的问题，如.  \n"
                             + "1- airwatch无法打开附件;  \n"
                             + "2- outlook无法收发邮件;  \n"
                             + "3- 发送到外部邮箱的邮件被阻止了;  \n"
                             + "4- 无法在公司电脑上打开私人邮箱;  \n"
                             + "5- 不能用group mail发送邮件;  \n"
                             + "6- 不能发送附件;  \n"
                             + "7- 移动设备上的邮件没法更新.  \n";
            await context.PostAsync(message);
            context.Wait(MessageReceived);
        }

        [LuisIntent("open")]
        public async Task Open(IDialogContext context, LuisResult result)
        {
            EntityRecommendation entity;
            string message = "";

            if(result.TryFindEntity("Atachment", out entity))
            {
                PromptDialog.Choice(
                    context,
                    AfterOpenAttachmentStep1,
                    new string[] {"Airwatch", "Mobile Iron"},
                    "您移动设备的邮件客户端是Airwatch，还是Mobile Iron ?"
                );
            }
            else if (result.TryFindEntity("PrivateMail", out entity))
            {
                await context.PostAsync("不可以啦！就是不可以啦！（点击这里阅读BD安全政策)");
                context.Wait(this.MessageReceived);
            }
        }

        public async Task AfterOpenAttachmentStep1(IDialogContext context, IAwaitable<string> argument)
        {
            var choice = await argument;
            if (choice == "Airwatch")
            {
                await context.PostAsync("请确认airwatch content已安装并输入了正确的账号密码（即电脑开机账号，密码）");
                context.Wait(this.MessageReceived);
            }
            else
            {
                await context.PostAsync("mobile iron 已经不再使用，请下载激活 airwatch.");
                context.Done(0);
            }
        }

        [LuisIntent("sendAndReceive")]
        public async Task sendAndReceive(IDialogContext context, LuisResult result)
        {
            EntityRecommendation entity;
            string message = "";

            if (result.TryFindEntity("Atachment", out entity))
            {
                message = $" 附件是否符合规范:  \n"
                         +"1. 公司内部附件小于等于15MB， 公司外部附件小于等于10MB;   \n" 
                         +"2. 同时附件不能带有宏.  \n"
                         +"仍无法发送请联系400.";
                await context.PostAsync(message);
            }
            else if (result.TryFindEntity("AirWatch", out entity))
            {
                PromptDialog.Choice(
                    context,
                    AfterChooseDevice,
                    new string[] { "IOS", "安卓" },
                    "您移动设备的系统是IOS，还是安卓 ?"
                );
            }
            else if (result.TryFindEntity("GroupMail", out entity))
            {
                PromptDialog.Choice(
                    context,
                    AfterGroupMailStep1,
                    new string[] { "是", "否" },
                    $"请先确认是否有权限，如无权限请先去ITONE申请权限。  \n"
                    + "是否无法找到发件人选项？"
                );
            }
            else if (result.TryFindEntity("Outlook", out entity))
            {
                PromptDialog.Choice(
                    context,
                    AfterOutlookSendStep1,
                    new string[] {"问题已解决", "问题未解决"},
                    $"请检查Outlook是否连接服务器?    \n"
                    + $"1. 显示试图连接: 请重启Outlook。  \n"
                    + $"2. 显示已断开： 请重启Outlook。  \n"
                    + $"3. 显示需要密码：直接点击按钮"                   
                );
            }
            else if (result.TryFindEntity("ExternalEmail", out entity))
            {
                PromptDialog.Choice(
                    context,
                    AfterExternalMailStep1,
                    new string[] {"是", "否"},
                    $"请先确认邮箱是否正确。   \n"
                    +$"您是否是属于R&D 员工? "
                    );
            }
            else
            {
                await context.PostAsync("Could you please be more specific?");
            }

        }

        public async Task AfterChooseDevice(IDialogContext context, IAwaitable<string> argument)
        {
            var choice = await argument;
            if (choice == "IOS")
            {
                PromptDialog.Choice(
                    context,
                    AfterChooseMailAppAsync,
                    new string[] { "Airwatch", "Mobile Iron" },
                    "您移动设备的邮件客户端是Airwatch，还是Mobile Iron ?"
                );
            }
            else
            {
                await context.PostAsync("我们暂时无法支持安卓系统，请联系400.");
                context.Done(0);
            }
        }

        public async Task AfterChooseMailAppAsync(IDialogContext context, IAwaitable<string> argument)
        {
            var choice = await argument;
            if (choice == "Airwatch")
            {
                PromptDialog.Choice(
                    context,
                    AfterAirwatchActivateAsync,
                    new string[] { "激活", "未激活" },
                    "您的airwatch是否处于激活状态 ?"
                );
            }
            else
            {
                await context.PostAsync("mobile iron 已经不再使用，请下载激活 airwatch.");
                context.Done(0);
            }
        }

        public async Task AfterAirwatchActivateAsync(IDialogContext context, IAwaitable<string> argument)
        {
            var choice = await argument;
            string message = "";
            if (choice == "激活")
            {
                message =
                    $"1. 您可能太久未使用了，以致airwatch自动retire。 您需要重新激活。\n"
                    +"2. 您的电脑密码可能过期。 请尝试在电脑上更新密码，然后在手机上点开设置->邮件->账户->BDO365->账户邮件地址-同步新密码";
            }
            else
            {
                message = "请先激活airwatch";
            }
            await context.PostAsync(message);
            context.Done(0);
        }

        public async Task AfterOutlookSendStep1(IDialogContext context, IAwaitable<string> argument)
        {
            var choice = await argument;
            if (choice == "问题未解决")
            {
                string message =
                    $"1. 是否是O365 用户 ?  \n"
                    + "--O365 用户请使用 GID@bd.com   \n"
                    + "2. 是否反复弹出密码对话框 ?   \n"
                    + "--请注意用户名是否正确，输入次数小于5次.  \n"
                    + "3. 能否正常上外网? 例如Google   \n"
                    + "--请检查IE代理（IE浏览器-- - Internet 选项->连接>局域网设置->第一第二个勾勾上。 \n"
                    + "4. 最近是否更新过电脑密码?   \n"
                    + "--如果最近更新过密码，现在又无法输入密码，请至开始->控制面板->右上角选择大图标，小图标，请勿选择类别->凭据管理器->删除所有凭据->重启outlook->输入开机密码 ? ";
                await context.PostAsync(message);
            }
            else
            {
                context.Done(0);
            }
        }

        public async Task AfterExternalMailStep1(IDialogContext context, IAwaitable<string> argument)
        {
            var choice = await argument;
            if (choice == "否")
            {
                PromptDialog.Choice(
                    context,
                    AfterExternalMailStep2,
                    new string[] { "是", "否" },
                    "邮件是否带附件？"
                );
            }
            else
            {
                await context.PostAsync("R&d 员工 外部邮箱被禁用，需要特殊申请，请联系BU助理了解详细流程。");
                context.Done(0);
            }
        }

        public async Task AfterExternalMailStep2(IDialogContext context, IAwaitable<string> argument)
        {
            var choice = await argument;
            string message = "";
            if (choice == "是")
            {
                message = "请确认附件是否符合附件规则：公司内部附件小于等于15MB，公司外部附件小于等于10MB";
            }
            else
            {
                message = "请联系400.";
            }
            await context.PostAsync(message);
            context.Done(0);
        }

        public async Task AfterGroupMailStep1(IDialogContext context, IAwaitable<string> argument)
        {
            var choice = await argument;
            string message = "";
            if (choice == "是")
            {
                message = "发件人选项： 邮件上方选项->发件人->输入公共邮箱地址.";
            }
            else
            {
                message = " 仍有问题请联系400. ";
            }
            await context.PostAsync(message);
            context.Done(0);
        }
    }
}   