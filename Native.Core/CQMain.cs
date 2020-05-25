using com.metricv.pcrguild.Code;
using Native.Sdk.Cqp.Interface;
using Unity;

namespace Native.Core {
    /// <summary>
    /// 酷Q应用主入口类
    /// </summary>
    public class CQMain
	{
		/// <summary>
		/// 在应用被加载时将调用此方法进行事件注册, 请在此方法里向 <see cref="IUnityContainer"/> 容器中注册需要使用的事件
		/// </summary>
		/// <param name="container">用于注册的 IOC 容器 </param>
		public static void Register (IUnityContainer unityContainer) {
			unityContainer.RegisterType<IAppEnable, InitEventHandlerDB>("应用已被启用");
			unityContainer.RegisterType<IAppDisable, InitEventHandlerDB>("应用将被停用");
			unityContainer.RegisterType<IGroupAddRequest, GroupAddEvent>("群添加请求处理");
			unityContainer.RegisterType<IPrivateMessage, MessageDigestor>("CommandDigestPrivate");
			unityContainer.RegisterType<IGroupMessage, MessageDigestor>("CommandDigestGroup");
			unityContainer.RegisterType<IDiscussMessage, MessageDigestor>("CommandDigestDiscuss");
		}
	}
}
