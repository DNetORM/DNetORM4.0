# DNetORM4.0

Instructions

http://www.cnblogs.com/DNetORM/p/8000373.html

DNetORM is a ORM framework based on.Net light weight and light configuration, the core code size is only 100K, SQLSERVER, MYSQL, ORACLE database support, the core idea of DNetORM is that the C# code mapping closest to the native the SQL statement to meet the scene, so as to improve the development efficiency of the greatest degree.


Why should the development of a ORM framework, the development of these years,i using a lot of orms, there are third parties such as mybatis, NHibernate, EF and so on, but they are not in line with my habits, my first love is the light configuration not love a large number of SQL statements written in the configuration file, nor love will be written in the entity mapping configuration file, so as mybatis, NHibernate this configuration ORM I do not love, EF is a powerful framework, but EF also has some defects, too heavy, also EF support for MySQL and Oracle are not friendly. In addition, there are some other frame interfaces which are not friendly, too complicated to write, or not clear interfaces. All these factors are prompting me to write a ORM that fits my development scenario.


DNetORM is very simple to use, and DNetORM refuses to encapsulate too many methods. This will cause the difficulty of using, but only encapsulates some commonly used methods of additions and deletions, and it will be very simple to use.


DNetORM for the multi table Lianzha of conventional package in support of LEFT JOIN, INNER JOIN, GROUP BY, ORDER BY, WHERE, for complex writing, recommend the use of DNetORM provided SQL query interface, we use the orm in development is to enhance the development efficiency, reduce development time, this is why the use of ORM,ORM is not a purpose in order to avoid the use of SQL, the actual development of many queries will be very complicated, if we rely on the ORM,even ORM support the first it will write very complex, not intuitive, ever when boys in the development of LINQ is the same with a more complex SQL, write a SQL it is 5 minutes, but in order to achieve the LINQ written for half an hour, even the  write is not correct, so these are contrary to the spirit of development. So ORM is a development tool, its appearance is to save development time and improve development efficiency, the use of ORM can help you solve the chores of additions and deletions, in the query, ORM help you deal with most of the single table query, in multi table query, ORM help you with simple multi table query, help you get the fastest data for complex SQL query using SQL query interface. Perhaps this is more reasonable.
