using AngusTools.LogHelper;
using Microsoft.VisualBasic.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngusTools.LogHelper
{
    public static class LogDemo
    {
        public static void TestShow()
        {
            LogManager.Info("记录一条Info等级的信息");
            //正常写入日志
            LogManager.Info("This is an info message: {0}", "TestInfo");
            LogManager.Debug("This is a debug message: {0}", "TestDebug");
            LogManager.Warn("This is an warning message: {0}", "TestInfo");
            LogManager.Error("This is a error message: {0}", "TestDebug");

            //自定义写入日志，一般情况下使用枚举定义日志记录器的名称
            var logger = LogManager.GetLogger("测试日志");
            logger.Info("This is an info message: {0}", "TestInfo");
            logger.Debug("This is a debug message: {0}", "TestDebug");
            logger.Warn("This is an warning message: {0}", "TestInfo");
            logger.Error("This is a error message: {0}", "TestDebug");

            // 在程序退出前关闭所有日志记录器，默认超时时间是3秒
            //LogManager.Close(5);

            //调试时偶尔使用
            if (LogManager.LastException != null)
                Console.WriteLine("日志异常:" + LogManager.LastException);

            //配置方法(一般情况下使用默认配置即可)：
            //自定义日志保存路径，默认保存到程序启动目录下的Log文件夹
            LogManager.CustomLogPath = () => AppDomain.CurrentDomain.BaseDirectory + "\\CustomLogs";

            //自定义日志文件名称，默认文件名为 DateTime.Now.ToString("yyyy-MM-dd") + ".log"
            LogManager.CustomLogFileName = () => "MyLog_" + DateTime.Now.ToString("yyyyMMdd") + ".log";

            //日志保存天数，默认30天
            LogManager.SaveDays = 10;

            //日志记录的格式，默认为 $"[{Time:yyyy-MM-dd HH:mm:ss ffff}] [{Level.ToString().ToUpper()}] [{ThreadId}] {Message}"
            LogManager.LogFormatter = (item) =>
            {
                //可以在这里做日志等级筛选，如果返回string.Empty这该条日志不会记录到文件
                return $"{item.Time:yyyy/MM/dd HH:mm:ss.fff} | {item.Level} | T{item.ThreadId:0000} | {item.Message}";
            };

            //日志回调，可用于界面实时显示日志或日志保存其它存储介质
            LogManager.OnWriteLog = (item) => Console.WriteLine("An event was logged: " + item.ToString());
        }
    }
}
