# HeightmapGeographicalInformation
>  从高度信息生成各种地理信息

## 源高度图

![](./Image/HeightMap.png)

## 法线图

![](./Image/NormalMap.png)

## 纵横图

![](./Image/AspectMap.png)

## 坡度图

![](./Image/SlopeMap.png)

## 流量图

![](./Image/FlowMap.png)

## 残差图

<center>海拔</center>

![ResidualMap_ELEVATION](./Image/ResidualMap_ELEVATION.png)


<center>平均值</center>

![ResidualMap_MEAN](./Image/ResidualMap_MEAN.png)


<center>标准差</center>

![ResidualMap_STDEV](./Image/ResidualMap_STDEV.png)


<center>偏差</center>

![ResidualMap_DEVIATION](./Image/ResidualMap_DEVIATION.png)


<center>百分数</center>

![ResidualMap_PERCENTILE](./Image/ResidualMap_PERCENTILE.png)


## 地形图

<center>堆积地貌</center>

![LandformMap_ACCUMULATION](./Image/LandformMap_ACCUMULATION.png)


<center>高斯</center>

![LandformMap_GAUSSIAN](./Image/LandformMap_GAUSSIAN.png)


<center>形状指数</center>

![LandformMap_SHAPE_INDEX](./Image/LandformMap_SHAPE_INDEX.png)


## 曲率图

<center>累积曲率</center>

![CurvatureMap_ACCUMULATION](./Image/CurvatureMap_ACCUMULATION.png)


<center>差异曲率</center>

![CurvatureMap_DIFFERENCE](./Image/CurvatureMap_DIFFERENCE.png)


<center>高斯曲率</center>

![CurvatureMap_GAUSSIAN](./Image/CurvatureMap_GAUSSIAN.png)


<center>水平曲率</center>

![CurvatureMap_HORIZONTAL](./Image/CurvatureMap_HORIZONTAL.png)


<center>水平超曲率</center>

![CurvatureMap_HORIZONTAL_EXCESS](./Image/CurvatureMap_HORIZONTAL_EXCESS.png)


<center>最大曲率</center>

![CurvatureMap_MAXIMAL](./Image/CurvatureMap_MAXIMAL.png)


<center>平均曲率</center>

![CurvatureMap_MEAN](./Image/CurvatureMap_MEAN.png)


<center>最小曲率</center>

![CurvatureMap_MINIMAL](./Image/CurvatureMap_MINIMAL.png)


<center>平面曲率</center>

![CurvatureMap_PLAN](./Image/CurvatureMap_PLAN.png)


<center>环曲率</center>

![CurvatureMap_RING](./Image/CurvatureMap_RING.png)


<center>转子曲率</center>

![CurvatureMap_ROTOR](./Image/CurvatureMap_ROTOR.png)


<center>非球面曲率</center>

![CurvatureMap_UNSPHERICITY](./Image/CurvatureMap_UNSPHERICITY.png)


<center>垂直曲率</center>

![CurvatureMap_VERTICAL](./Image/CurvatureMap_VERTICAL.png)


<center>垂直超曲率</center>

![CurvatureMap_VERTICAL_EXCESS](./Image/CurvatureMap_VERTICAL_EXCESS.png)


算法来自[这里](https://github.com/Scrawk/Terrain-Topology-Algorithms)，我创建了这些算法的JobSystem版本，计算速度可以支持在运行时实时生成。
