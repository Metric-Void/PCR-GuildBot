## PCR行会战机器人插件
PCR工会战机器人基于C# Native.Sdk。用户可使用指令登记模拟战信息，统计战况，给出预估伤害值，并使用预估的伤害数值进行排刀。    
<s>我不是很擅长C# 但是酷Q的Java SDK实属拉跨 饶了我吧（</s>

### 下载
[GitHub Releases](https://github.com/Metric-Void/PCR-GuildBot/releases)

### 伤害预估机制
伤害预估基于Gaussian分布。算法使用ML Estimation，基于用户的模拟战数据构建Gaussian分布模型，根据Central Limit Theorem构建各个概率下的预估值，并根据Chebyshev Inequality，给出各个预估值的置信度。用户输入的模拟战数据越多，预估值的置信度越高。

### 排刀机制
令N为参与排刀的队伍数量，K为选中的队伍数量，M为boss的血量。使用动态规划的话时间效率大约为O(Nlog(M))。   
但是因为M值本身较大，而K值较小，因此使用了伪多项式算法，时间效率为O(NK)，<s>空间效率不是很好看</s>

### 使用方法/指令表
参见[这里](usage.md)

### 更新日志
v1.0.5
- 修复了“统计”指令有时无法识别参数的bug

v1.0.4
- 增加指令宽容度。

v1.0.3
- 群主以及群管理员 默认给予插件的权限。
