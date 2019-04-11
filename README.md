# Unity MapGen

 基于多边形的随机地图生成。

![](./images/mapGen_01.png)

### Part1 Polygon

随机地图的生成首先需要一张高度图(Elevation)，可以理解为海拔数据代表了每个坐标的海拔值。再用一张降雨图(Moisture)来影响植被和河流。高度图一般的做法是使用噪音来得到，而降雨图是根据高度图计算得来的(海岸和季风的影响)。简单的户外环境使用噪音即可以得到比较自然的地形，但实际游戏需求往往是复杂的。可能需要选择在不同的位置生成不同的植物，动物群落。再或者游戏的资源的投放是需要关联不同地貌的，例如在沙漠地带投放水资源相关掉落。在平原地区生成城市，在热带雨林生成部落等。

我们使用多边形来填充地图，相比传统的基于Tile的地图生成在很大程度上降低了空间上的复杂度。100000个Tile的地图可能只需要1000个多边形就可以完成地图数据的填充。

#### Voronoi Diagram (沃罗诺伊图)

首先我们使用**Voronoi Diagram**来分割地图。沃罗诺伊图(Voronoi Diagram)又叫泰森多边形，该算法实现使用一组特定的点将平面分割成不同区域，而每一区域又仅包含唯一的特定点，并且该区域内任意位置到该特定点的距离最小。分割后得到的Voronoi节点可有多种适用场景，如在地图的平原地区生成一个面积最大的城镇等。为了使用Voronoi来分割地图，要先生成一些随机点，随机点的生成可以是柏林噪音，或其他任何随机，为了能够利用种子(seed)来还原地图，这里最好使用伪随机来生成点。

![](./images/mapGen_02.png)

随机生成了1000个点，然后我们使用**Delaunay三角剖分**来得到三角形。这里使用基于**Fortune's algorithm**的三角剖分。

![](./images/mapGen_05.png)

为了方便观察，我把随机的点数调整为500个。接下来用已有的三角形得到Voronoi多边形。

![](./images/mapGen_03.png)

绿色的多边形，即为Voronoi多边形。我们仔细观察一下单个多边形：

![](./images/mapGen_07.png)

可以看到一个三角形对应一个多边形的顶角，每个多边形又对应三角形的一个顶角。这种双重性将会用在不同的地方，比如三角形可以用来寻路，而多边形则可以用来渲染。后面会有具体的应用场景。现在我们把暂时用不到的三角形的渲染关掉，再来观察这些多边形：

![](./images/mapGen_04.png)

 可以看到多边形的大小都非常不规则，接下来我们使用**Lloyd relaxation**调整网格大小使整体更规整:

![](./images/mapGen_06.png)



> Voronoi Diagram: <https://en.wikipedia.org/wiki/Voronoi_diagram>
>
> Fortune's algorithm：<https://en.wikipedia.org/wiki/Fortune's_algorithm>
>
> Lloyd relaxation: <https://en.wikipedia.org/wiki/Lloyd%27s_algorithm>

#### 区分水和陆地

现在要生成一个被大海包围的岛屿，这里我们使用**Radial**正弦波算法来生成圆形岛屿。当然岛屿可以是任何条件约束下生成的形状，代码里提供了另外两种：Perlin和Square算法来生成岛屿。

![](./images/mapGen_08.png)

#### 区分海洋,海岸线和内陆湖

![](./images/mapGen_09.png)

1. 首先规定地图边缘均为海洋 。
2. 所有与海洋接触的水均为海洋。
3. 所有与海洋接触的陆地为海岸线即渲染为沙滩。
4. 其余的水均为湖泊。 

### Part2 海拔，河流，湿度，生物类群

#### 海拔(Elevation)
接下来给每个网格生成海拔值。海拔值会是很重要的数据，它将会用来影响生物类群的分布，河流的流向等。首先通过Voronoi多边形的角距离海岸线的距离来确定其海拔值，最后将Voronoi多边形所有角海拔的平均值存储到Voronoi多边形对应的**Center**中。
1. `AssignCornerElevations`方法为每个角计算海拔值
   从边缘的角开始计算海拔值，并通过当前角的相邻角依次往里收缩计算角的海拔值。

   ```c#
   // queue存储了边缘角
       while(queue.Count > 0)
        {
            Corner q = queue.Pop();
   
            foreach(Corner s in q.Adjacent) // 从边缘角通过相邻角向里收缩 
            {
                double newElevation = 0.01f + q.Elevation;
                if(!q.Water && !s.Water)
                {
                    newElevation += 1;
                    if(NeedsMoreRandomness)
                    {
                        // 如果是Square或Hexagon类型地图，海拔加入随机以确保后面河流的生成有足够的随机性
                        newElevation += ParkMillerRng.NextDouble();
                    }
                }
   
                if (newElevation < s.Elevation)
                {
                    s.Elevation = newElevation;
                    queue.Push(s);
                }
            }
        }
   ```

2. `AssignPolygonElevations`方法计算海拔平均值
   ```c#
   // 计算平均值
   public void AssignPolygonElevations()
    {
        double sumElevation;
        foreach(Center p in Centers)
        {
            sumElevation = 0.0;
            foreach(Corner q in p.Corners)
            {
                sumElevation += q.Elevation;
            }
   
            p.Elevation = sumElevation / (double)p.Corners.Count;
        }
    }
   ```

为了方便观察海拔的分布，将海拔值转换为灰度值来绘制。颜色取值0-255，越接近白色代表海拔越高。
![](./images/mapGen_10.png)

海拔对河流的影响: 河流会从高海拔流入低海拔地区，并最终汇入大海。
海拔对生物群落的影响: 高海拔地区会分布岩石，雪，冻土地带。中海拔地区会分布灌木，沙漠，森林和草原。低海拔地区会分布雨林，草原，海滩。

Rivers
Rivers and lakes are the two fresh water features I wanted. The most realistic approach would be to define moisture with wind, clouds, humidity, and rainfall, and then define the rivers and lakes based on where it rains. Instead, I'm starting with the goal, which is good rivers, and working backwards from there.

The island shape determines which areas are water and which are land. Lakes are water polygons that aren't oceans.

Rivers use the downhill directions shown earlier. I choose random corner locations in the mountains, and then follow a path down to the ocean. The rivers flow from corner to corner:

Elevation map with one river
I tried both polygon centers and corners, but found that the corner graph made for much nicer looking rivers. Also, by keeping lakes flat, elevation tends to be lower near lakes, so rivers naturally flow into and out of lakes. Multiple rivers can share the lower portion of their path, but once they join, they never diverge, so tributary formation comes for free. It's simple and seems to work pretty well.

Moisture
Since I'm working backwards, I don't need moisture to form rivers. However, moisture would be useful for defining biomes (deserts, swamps, forests, etc.). Since rivers and lakes should form in areas with high moisture, I defined moisture based on distance from fresh water:

Moisture map
As with elevation, I redistribute moisture to match a desired distribution. In this case, I want roughly equal numbers of dry and wet regions. In this map generator, moisture is only used for biomes. However, games may find other uses for the moisture data. For example, Realm of the Mad God uses moisture and elevation to distribute vegetation.

Biomes
Together, elevation and moisture provide a good amount of variety to define biome types. I use elevation as a proxy for temperature. Biomes first depend on whether it's water or land:

Ocean is any water polygon connected to the map border
Lake is any water polygon not connected to the map border
Ice lake if the lake is at high elevation (low temperature)
Marsh if it's at low elevation
Beach is any land polygon next to an ocean
For all land polygons, I started with the Whittaker diagram and adapted it to my needs:

Elevation	Moisture
Wet					Dry
High	Snow	Tundra	Bare rock	Scorched
Medium-high	Taiga	Shrubland	Temperate desert
Medium-low	Temperate rain forest	Temperate deciduous forest	Grassland	Temperate desert
Low	Tropical rain forest	Tropical seasonal forest	Grassland	Subtropical desert
Here's the result:

Biome map
These biomes look good in the map generation demo, but each game will have its own needs. Realm of the Mad God for example ignores these biomes and uses its own (based on elevation and moisture).

In the last blog post I'll describe how I get from this biome map to maps like this: (or even this!)

Goal of the map generation
Update: [2010-09-22] I replaced the last diagram on this page with what I originally wanted but didn't finish in time for the blog post. At the time of posting, I used this image instead.

Labels:  maps ,  project ,  voronoi

– Amit – Sunday, September 05, 2010
6 comments:
felix wrote at September 06, 2010 12:34 AM
Wow! Fantastic use of Voronoi diagrams. For some reason I prefer the 2nd to last hexagonal shapes over the last more detailed image. Maybe because it looks like a cool boardgame.

Emile wrote at September 06, 2010 1:49 AM
It looks like you switched the axis for moisture - I'm pretty sure deserts don't count as "high moisture" :)

Interesting stuff! I used something like your whittaker diagram for world generation, except that I had a third dimension, temperature.

Amit wrote at September 06, 2010 7:51 AM
Hi Felix — yes, I think the polygons could be very useful for many games, especially when the polygons are directly used by the player, such as in a settlement or territory conquest game. The board game “Risk” is drawn with fancy borders but at its heart is a graph based game, where you can move only from node to node along specific borders. My goal for the next blog post is to have some fancy borders, but still keep the polygon map underneath. In games where the polygons matter, you'd want to draw the borders, as in this image.

Amit wrote at September 06, 2010 8:17 AM
Hi Emile — yes, good catch! Thank you; I have corrected the table.

Temperature, wind, humidity, variable intensity rain, vegetation, rain shadows, flooding, erosion, evaporation, aquifers, soil dampness, soil permeability might all be useful for maps. I played with some of these in previous map projects and decided this time I'm going to keep it simple. :)

BTW there's an absolutely fantastic map generation blog that you may enjoy reading: Dungeon League. They're generating much more sophisticated maps than I am in this project. Also see this discussion on TIGsource about the Dungeon League maps.

Piiichan wrote at September 18, 2010 1:27 AM
Hey Amit,
very interesting post.

I want to learn more about how you control distribution of features.

For example you said:
"As with elevation, I redistribute moisture to match a desired distribution."

How do you do that?

Thanks

Amit wrote at September 18, 2010 7:57 PM
Hi Piiichan, I did something very simple with moisture: I sorted the corners by moisture, then reassigned the moistures to be from 0.0 to 1.0, evenly. For example if there are 200 corners, then corner i will get moisture i/200. See the redistributeMoisture function in the source.

Post a Comment

Newer Post Older Post Home
Subscribe to: Post Comments ( Atom )
Copyright © 2019 Red Blob Games