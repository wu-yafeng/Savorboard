# Savorborad

这是一个用 Microsoft.Orleans 库实现的 Virutal Actor 模型的游戏服务器项目，以及一个简单的控制台客户端。 

## 基本功能

### 服务器帧

该游戏实现了服务器帧的概念。
- 给在线的玩家每x秒提升x点会员经验，并在会员等级提升时，给玩家发放一件装备道具，并通知给客户端。
- GameMap会随机生成若干个怪物，并会攻击同地图的所有玩家。当怪物生成的时候，会通知同地图的玩家，有新的怪物生成。当受到伤害时，受到伤害的玩家会在客户端接收到通知

### 会话

- 客户端需要定时向服务器发送心跳包，确保玩家的在线状态。在每个心跳包时，会重新订阅客户端关注的事件（比如地图、怪物、玩家发生的事件）
- 服务端使用ObserverManager 来管理在线状态，当超过X时间未收到消息，将玩家从订阅者中踢出。

### 数值管理

- 全局管理对象 IMetaMgr 是全局单例的，用于管理所有的数值表
- IServiceClient<IMetaService> 是一个针对IMetaMgr的拷贝副本，用于增加Grain访问数值表的速度。同样，IMetaService会订阅 IMetaMgr 对象，当发生变化时，会更新本地的副本。尽管大部分情况下 IServiceClient<IMetaService> 总是访问本地副本，但不承诺。  
