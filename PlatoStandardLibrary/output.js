class Interval_133_Library
{
    static Size_1490 = function (x_1489) { return Subtract_171(Max_1297(x_1489), Min_1295(x_1489)); };
    static IsEmpty_1492 = function (x_1491) { return GreaterThanOrEquals_1293(Min_1295(x_1491), Max_1297(x_1491)); };
    static Lerp_1495 = function (x_1493, amount_1494) { return Multiply_173(Min_1295(x_1493), Add_169(Subtract_171(1, amount_1494), Multiply_173(Max_1297(x_1493), amount_1494))); };
    static InverseLerp_1498 = function (x_1496, value_1497) { return Divide_175(Subtract_171(value_1497, Min_1295(x_1496)), Size_1115(x_1496)); };
    static Negate_1500 = function (x_1499) { return Tuple_1(Negative_158(Max_1297(x_1499)), Negative_158(Min_1295(x_1499))); };
    static Reverse_1502 = function (x_1501) { return Tuple_1(Max_1297(x_1501), Min_1295(x_1501)); };
    static Resize_1505 = function (x_1503, size_1504) { return Tuple_1(Min_1295(x_1503), Add_169(Min_1295(x_1503), size_1504)); };
    static Center_1507 = function (x_1506) { return Lerp_1119(x_1506, 0.5); };
    static Contains_1510 = function (x_1508, value_1509) { return LessThanOrEquals_1289(Min_1295(x_1508), And_179(value_1509, LessThanOrEquals_1289(value_1509, Max_1297(x_1508)))); };
    static Contains_1513 = function (x_1511, other_1512) { return LessThanOrEquals_1289(Min_1295(x_1511), And_179(Min_1295(other_1512), GreaterThanOrEquals_1293(Max_1297, Max_1297(other_1512)))); };
    static Overlaps_1516 = function (x_1514, y_1515) { return Not_183(IsEmpty_1323(Clamp_1215(x_1514, y_1515))); };
    static Split_1519 = function (x_1517, t_1518) { return Tuple_1(Left_1141(x_1517, t_1518), Right_1143(x_1517, t_1518)); };
    static Split_1521 = function (x_1520) { return Split_1139(x_1520, 0.5); };
    static Left_1524 = function (x_1522, t_1523) { return Tuple_1(Min_1295, Lerp_1119(x_1522, t_1523)); };
    static Right_1527 = function (x_1525, t_1526) { return Tuple_1(Lerp_1119(x_1525, t_1526), Max_1297(x_1525)); };
    static MoveTo_1530 = function (x_1528, t_1529) { return Tuple_1(t_1529, Add_169(t_1529, Size_1115(x_1528))); };
    static LeftHalf_1532 = function (x_1531) { return Left_1141(x_1531, 0.5); };
    static RightHalf_1534 = function (x_1533) { return Right_1143(x_1533, 0.5); };
    static HalfSize_1536 = function (x_1535) { return Half_1227(Size_1115(x_1535)); };
    static Recenter_1539 = function (x_1537, c_1538) { return Tuple_1(Subtract_171(c_1538, HalfSize_1151(x_1537)), Add_169(c_1538, HalfSize_1151(x_1537))); };
    static Clamp_1542 = function (x_1540, y_1541) { return Tuple_1(Clamp_1215(x_1540, Min_1295(y_1541)), Clamp_1215(x_1540, Max_1297(y_1541))); };
    static Clamp_1545 = function (x_1543, value_1544) { return LessThan_1283(value_1544, Min_1295(x_1543)
        ? Min_1295(x_1543)
        : GreaterThan_1291(value_1544, Max_1297(x_1543)
            ? Max_1297(x_1543)
            : value_1544
        )
    ); };
    static Between_1548 = function (x_1546, value_1547) { return GreaterThanOrEquals_1293(value_1547, And_179(Min_1295(x_1546), LessThanOrEquals_1289(value_1547, Max_1297(x_1546)))); };
    static Unit_1549 = function () { return Tuple_1(0, 1); };
}
class Vector_134_Library
{
    static Sum_1551 = function (v_1550) { return Aggregate_1319(v_1550, 0, Add_169); };
    static SumSquares_1553 = function (v_1552) { return Aggregate_1319(Square_1209(v_1552), 0, Add_169); };
    static LengthSquared_1555 = function (v_1554) { return SumSquares_1165(v_1554); };
    static Length_1557 = function (v_1556) { return SquareRoot_1205(LengthSquared_1167(v_1556)); };
    static Dot_1560 = function (v1_1558, v2_1559) { return Sum_1163(Multiply_173(v1_1558, v2_1559)); };
}
class Numerical_135_Library
{
    static Cos_1562 = function (x_1561) { return intrinsic_0; };
    static Sin_1564 = function (x_1563) { return intrinsic_0; };
    static Tan_1566 = function (x_1565) { return intrinsic_0; };
    static Acos_1568 = function (x_1567) { return intrinsic_0; };
    static Asin_1570 = function (x_1569) { return intrinsic_0; };
    static Atan_1572 = function (x_1571) { return intrinsic_0; };
    static Cosh_1574 = function (x_1573) { return intrinsic_0; };
    static Sinh_1576 = function (x_1575) { return intrinsic_0; };
    static Tanh_1578 = function (x_1577) { return intrinsic_0; };
    static Acosh_1580 = function (x_1579) { return intrinsic_0; };
    static Asinh_1582 = function (x_1581) { return intrinsic_0; };
    static Atanh_1584 = function (x_1583) { return intrinsic_0; };
    static Pow_1587 = function (x_1585, y_1586) { return intrinsic_0; };
    static Log_1590 = function (x_1588, y_1589) { return intrinsic_0; };
    static NaturalLog_1592 = function (x_1591) { return intrinsic_0; };
    static NaturalPower_1594 = function (x_1593) { return intrinsic_0; };
    static SquareRoot_1596 = function (x_1595) { return Pow_1197(x_1595, 0.5); };
    static CubeRoot_1598 = function (x_1597) { return Pow_1197(x_1597, 0.5); };
    static Square_1600 = function (x_1599) { return Multiply_173(Value_1105, Value_1105); };
    static Clamp_1604 = function (x_1601, min_1602, max_1603) { return Clamp_1215(x_1601, Interval_133_Library(min_1602, max_1603)); };
    static Clamp_1607 = function (x_1605, i_1606) { return Clamp_1215(i_1606, x_1605); };
    static Clamp_1609 = function (x_1608) { return Clamp_1215(x_1608, 0, 1); };
    static PlusOne_1611 = function (x_1610) { return Add_169(x_1610, 1); };
    static MinusOne_1613 = function (x_1612) { return Subtract_171(x_1612, 1); };
    static FromOne_1615 = function (x_1614) { return Subtract_171(1, x_1614); };
    static Sign_1617 = function (x_1616) { return LessThan_1283(x_1616, 0
        ? Negative_158(1)
        : GreaterThan_1291(x_1616, 0
            ? 1
            : 0
        )
    ); };
    static Abs_1619 = function (x_1618) { return LessThan_1283(Value_1105, 0
        ? Negative_158(Value_1105)
        : Value_1105
    ); };
    static Half_1621 = function (x_1620) { return Divide_175(x_1620, 2); };
    static Third_1623 = function (x_1622) { return Divide_175(x_1622, 3); };
    static Quarter_1625 = function (x_1624) { return Divide_175(x_1624, 4); };
    static Fifth_1627 = function (x_1626) { return Divide_175(x_1626, 5); };
    static Sixth_1629 = function (x_1628) { return Divide_175(x_1628, 6); };
    static Seventh_1631 = function (x_1630) { return Divide_175(x_1630, 7); };
    static Eighth_1633 = function (x_1632) { return Divide_175(x_1632, 8); };
    static Ninth_1635 = function (x_1634) { return Divide_175(x_1634, 9); };
    static Tenth_1637 = function (x_1636) { return Divide_175(x_1636, 10); };
    static Sixteenth_1639 = function (x_1638) { return Divide_175(x_1638, 16); };
    static Hundredth_1641 = function (x_1640) { return Divide_175(x_1640, 100); };
    static Thousandth_1643 = function (x_1642) { return Divide_175(x_1642, 1000); };
    static Millionth_1645 = function (x_1644) { return Divide_175(x_1644, Divide_175(1000, 1000)); };
    static Billionth_1647 = function (x_1646) { return Divide_175(x_1646, Divide_175(1000, Divide_175(1000, 1000))); };
    static Hundred_1649 = function (x_1648) { return Multiply_173(x_1648, 100); };
    static Thousand_1651 = function (x_1650) { return Multiply_173(x_1650, 1000); };
    static Million_1653 = function (x_1652) { return Multiply_173(x_1652, Multiply_173(1000, 1000)); };
    static Billion_1655 = function (x_1654) { return Multiply_173(x_1654, Multiply_173(1000, Multiply_173(1000, 1000))); };
    static Twice_1657 = function (x_1656) { return Multiply_173(x_1656, 2); };
    static Thrice_1659 = function (x_1658) { return Multiply_173(x_1658, 3); };
    static SmoothStep_1661 = function (x_1660) { return Multiply_173(Square_1209(x_1660), Subtract_171(3, Twice_1263(x_1660))); };
    static Pow2_1663 = function (x_1662) { return Multiply_173(x_1662, x_1662); };
    static Pow3_1665 = function (x_1664) { return Multiply_173(Pow2_1269(x_1664), x_1664); };
    static Pow4_1667 = function (x_1666) { return Multiply_173(Pow3_1271(x_1666), x_1666); };
    static Pow5_1669 = function (x_1668) { return Multiply_173(Pow4_1273(x_1668), x_1668); };
    static Turns_1671 = function (x_1670) { return Multiply_173(x_1670, Multiply_173(3.1415926535897, 2)); };
    static AlmostZero_1673 = function (x_1672) { return LessThan_1283(Abs_1225(x_1672), 1E-08); };
}
class Comparable_136_Library
{
    static Equals_1676 = function (a_1674, b_1675) { return Equals_1281(Compare_152(a_1674, b_1675), 0); };
    static LessThan_1679 = function (a_1677, b_1678) { return LessThan_1283(Compare_152(a_1677, b_1678), 0); };
    static Lesser_1682 = function (a_1680, b_1681) { return LessThanOrEquals_1289(a_1680, b_1681)
        ? a_1680
        : b_1681
    ; };
    static Greater_1685 = function (a_1683, b_1684) { return GreaterThanOrEquals_1293(a_1683, b_1684)
        ? a_1683
        : b_1684
    ; };
    static LessThanOrEquals_1688 = function (a_1686, b_1687) { return LessThanOrEquals_1289(Compare_152(a_1686, b_1687), 0); };
    static GreaterThan_1691 = function (a_1689, b_1690) { return GreaterThan_1291(Compare_152(a_1689, b_1690), 0); };
    static GreaterThanOrEquals_1694 = function (a_1692, b_1693) { return GreaterThanOrEquals_1293(Compare_152(a_1692, b_1693), 0); };
    static Min_1697 = function (a_1695, b_1696) { return LessThan_1283(a_1695, b_1696)
        ? a_1695
        : b_1696
    ; };
    static Max_1700 = function (a_1698, b_1699) { return GreaterThan_1291(a_1698, b_1699)
        ? a_1698
        : b_1699
    ; };
    static Between_1704 = function (v_1701, a_1702, b_1703) { return Between_1301(v_1701, Interval_133_Library(a_1702, b_1703)); };
    static Between_1707 = function (v_1705, i_1706) { return Contains_1133(i_1706, v_1705); };
}
class Boolean_137_Library
{
    static XOr_1710 = function (a_1708, b_1709) { return a_1708
        ? Not_183(b_1709)
        : b_1709
    ; };
    static NAnd_1713 = function (a_1711, b_1712) { return Not_183(And_179(a_1711, b_1712)); };
    static NOr_1716 = function (a_1714, b_1715) { return Not_183(Or_181(a_1714, b_1715)); };
}
class Equatable_138_Library
{
    static NotEquals_1718 = function (x_1717) { return Not_183(Equals_1281(x_1717)); };
}
class Array_139_Library
{
    static Map_1723 = function (xs_1719, f_1720) { return Map_1311(Count_26_Type(xs_1719), function (i_1721) { return f_1720(At_213(xs_1719, i_1721)); }); };
    static Zip_1729 = function (xs_1724, ys_1725, f_1726) { return Array_139_Library(Count_26_Type(xs_1724), function (i_1727) { return f_1726(At_213(i_1727), At_213(ys_1725, i_1727)); }); };
    static Skip_1734 = function (xs_1730, n_1731) { return Array_139_Library(Subtract_171(Count_26_Type, n_1731), function (i_1732) { return At_213(Subtract_171(i_1732, n_1731)); }); };
    static Take_1739 = function (xs_1735, n_1736) { return Array_139_Library(n_1736, function (i_1737) { return At_213; }); };
    static Aggregate_1743 = function (xs_1740, init_1741, f_1742) { return IsEmpty_1323(xs_1740)
        ? init_1741
        : f_1742(init_1741, f_1742(Rest_1321(xs_1740)))
    ; };
    static Rest_1745 = function (xs_1744) { return Skip_1315(1); };
    static IsEmpty_1747 = function (xs_1746) { return Equals_1281(Count_26_Type(xs_1746), 0); };
    static First_1749 = function (xs_1748) { return At_213(xs_1748, 0); };
    static Last_1751 = function (xs_1750) { return At_213(xs_1750, Subtract_171(Count_26_Type(xs_1750), 1)); };
    static Slice_1755 = function (xs_1752, from_1753, count_1754) { return Take_1317(Skip_1315(xs_1752, from_1753), count_1754); };
    static Join_1761 = function (xs_1756, sep_1757) { return IsEmpty_1323(xs_1756)
        ? ""
        : Add_169(ToString_203(First_1325(xs_1756)), Aggregate_1319(Skip_1315(xs_1756, 1), "", function (acc_1758, cur_1759) { return Interpolate_1403(acc_1758, sep_1757, cur_1759); }))
    ; };
    static All_1764 = function (xs_1762, f_1763) { return IsEmpty_1323(xs_1762)
        ? True
        : And_179(f_1763(First_1325(xs_1762)), f_1763(Rest_1321(xs_1762)))
    ; };
    static JoinStrings_1770 = function (xs_1765, sep_1766) { return IsEmpty_1323(xs_1765)
        ? ""
        : Add_169(First_1325(xs_1765), Aggregate_1319(Rest_1321(xs_1765), "", function (x_1767, acc_1768) { return Add_169(acc_1768, Add_169(", ", ToString_203(x_1767))); }))
    ; };
}
class Easings_140_Library
{
    static BlendEaseFunc_1774 = function (p_1771, easeIn_1772, easeOut_1773) { return LessThan_1283(p_1771, 0.5
        ? Multiply_173(0.5, easeIn_1772(Multiply_173(p_1771, 2)))
        : Multiply_173(0.5, Add_169(easeOut_1773(Multiply_173(p_1771, Subtract_171(2, 1))), 0.5))
    ); };
    static InvertEaseFunc_1777 = function (p_1775, easeIn_1776) { return Subtract_171(1, easeIn_1776(Subtract_171(1, p_1775))); };
    static Linear_1779 = function (p_1778) { return p_1778; };
    static QuadraticEaseIn_1781 = function (p_1780) { return Pow2_1269(p_1780); };
    static QuadraticEaseOut_1783 = function (p_1782) { return InvertEaseFunc_1339(p_1782, QuadraticEaseIn_1343); };
    static QuadraticEaseInOut_1785 = function (p_1784) { return BlendEaseFunc_1337(p_1784, QuadraticEaseIn_1343, QuadraticEaseOut_1345); };
    static CubicEaseIn_1787 = function (p_1786) { return Pow3_1271(p_1786); };
    static CubicEaseOut_1789 = function (p_1788) { return InvertEaseFunc_1339(p_1788, CubicEaseIn_1349); };
    static CubicEaseInOut_1791 = function (p_1790) { return BlendEaseFunc_1337(p_1790, CubicEaseIn_1349, CubicEaseOut_1351); };
    static QuarticEaseIn_1793 = function (p_1792) { return Pow4_1273(p_1792); };
    static QuarticEaseOut_1795 = function (p_1794) { return InvertEaseFunc_1339(p_1794, QuarticEaseIn_1355); };
    static QuarticEaseInOut_1797 = function (p_1796) { return BlendEaseFunc_1337(p_1796, QuarticEaseIn_1355, QuarticEaseOut_1357); };
    static QuinticEaseIn_1799 = function (p_1798) { return Pow5_1275(p_1798); };
    static QuinticEaseOut_1801 = function (p_1800) { return InvertEaseFunc_1339(p_1800, QuinticEaseIn_1361); };
    static QuinticEaseInOut_1803 = function (p_1802) { return BlendEaseFunc_1337(p_1802, QuinticEaseIn_1361, QuinticEaseOut_1363); };
    static SineEaseIn_1805 = function (p_1804) { return InvertEaseFunc_1339(p_1804, SineEaseOut_1369); };
    static SineEaseOut_1807 = function (p_1806) { return Sin_1175(Turns_1277(Quarter_1231(p_1806))); };
    static SineEaseInOut_1809 = function (p_1808) { return BlendEaseFunc_1337(p_1808, SineEaseIn_1367, SineEaseOut_1369); };
    static CircularEaseIn_1811 = function (p_1810) { return FromOne_1221(SquareRoot_1205(FromOne_1221(Pow2_1269(p_1810)))); };
    static CircularEaseOut_1813 = function (p_1812) { return InvertEaseFunc_1339(p_1812, CircularEaseIn_1373); };
    static CircularEaseInOut_1815 = function (p_1814) { return BlendEaseFunc_1337(p_1814, CircularEaseIn_1373, CircularEaseOut_1375); };
    static ExponentialEaseIn_1817 = function (p_1816) { return AlmostZero_1279(p_1816)
        ? p_1816
        : Pow_1197(2, Multiply_173(10, MinusOne_1219(p_1816)))
    ; };
    static ExponentialEaseOut_1819 = function (p_1818) { return InvertEaseFunc_1339(p_1818, ExponentialEaseIn_1379); };
    static ExponentialEaseInOut_1821 = function (p_1820) { return BlendEaseFunc_1337(p_1820, ExponentialEaseIn_1379, ExponentialEaseOut_1381); };
    static ElasticEaseIn_1823 = function (p_1822) { return Multiply_173(13, Multiply_173(Turns_1277(Quarter_1231(p_1822)), Sin_1175(Radians_729(Pow_1197(2, Multiply_173(10, MinusOne_1219(p_1822))))))); };
    static ElasticEaseOut_1825 = function (p_1824) { return InvertEaseFunc_1339(p_1824, ElasticEaseIn_1385); };
    static ElasticEaseInOut_1827 = function (p_1826) { return BlendEaseFunc_1337(p_1826, ElasticEaseIn_1385, ElasticEaseOut_1387); };
    static BackEaseIn_1829 = function (p_1828) { return Subtract_171(Pow3_1271(p_1828), Multiply_173(p_1828, Sin_1175(Turns_1277(Half_1227(p_1828))))); };
    static BackEaseOut_1831 = function (p_1830) { return InvertEaseFunc_1339(p_1830, BackEaseIn_1391); };
    static BackEaseInOut_1833 = function (p_1832) { return BlendEaseFunc_1337(p_1832, BackEaseIn_1391, BackEaseOut_1393); };
    static BounceEaseIn_1835 = function (p_1834) { return InvertEaseFunc_1339(p_1834, BounceEaseOut_1399); };
    static BounceEaseOut_1837 = function (p_1836) { return LessThan_1283(p_1836, Divide_175(4, 11))
        ? Multiply_173(121, Divide_175(Pow2_1269(p_1836), 16))
        : LessThan_1283(p_1836, Divide_175(8, 11))
            ? Divide_175(363, Multiply_173(40, Subtract_171(Pow2_1269(p_1836), Divide_175(99, Multiply_173(10, Add_169(p_1836, Divide_175(17, 5)))))))
            : LessThan_1283(p_1836, Divide_175(9, 10))
                ? Divide_175(4356, Multiply_173(361, Subtract_171(Pow2_1269(p_1836), Divide_175(35442, Multiply_173(1805, Add_169(p_1836, Divide_175(16061, 1805)))))))
                : Divide_175(54, Multiply_173(5, Subtract_171(Pow2_1269(p_1836), Divide_175(513, Multiply_173(25, Add_169(p_1836, Divide_175(268, 25)))))))


    ; };
    static BounceEaseInOut_1839 = function (p_1838) { return BlendEaseFunc_1337(p_1838, BounceEaseIn_1397, BounceEaseOut_1399); };
}
class Intrinsics_141_Library
{
    static Interpolate_1841 = function (xs_1840) { return intrinsic_0; };
    static Throw_1843 = function (x_1842) { return intrinsic_0; };
    static TypeOf_1845 = function (x_1844) { return intrinsic_0; };
    static New_1847 = function (x_1846) { return intrinsic_0; };
}
class Vector_13_Concept
{
    constructor(self) { this.Self = self; };
    static Count_1412 = function (v_1411) { return Count_26_Type(FieldTypes_187(Self_7_Primitive)); };
    static At_1415 = function (v_1413, n_1414) { return At_213(FieldValues_191(v_1413), n_1414); };
}
class Measure_14_Concept
{
    constructor(self) { this.Self = self; };
    static Value_1417 = function (x_1416) { return At_213(FieldValues_191(x_1416), 0); };
}
class Numerical_15_Concept
{
    constructor(self) { this.Self = self; };
}
class Magnitude_16_Concept
{
    constructor(self) { this.Self = self; };
    static Magnitude_1419 = function (x_1418) { return SquareRoot_1205(Sum_1163(Square_1209(FieldValues_191(x_1418)))); };
}
class Comparable_17_Concept
{
    constructor(self) { this.Self = self; };
    static Compare_1422 = function (a_1420, b_1421) { return LessThan_1283(Magnitude_150(a_1420), Magnitude_150(b_1421)
        ? Negative_158(1)
        : GreaterThan_1291(Magnitude_150(a_1420), Magnitude_150(b_1421)
            ? 1
            : 0
        )
    ); };
}
class Equatable_18_Concept
{
    constructor(self) { this.Self = self; };
    static Equals_1425 = function (a_1423, b_1424) { return All_1333(Equals_1281(FieldValues_191(a_1423), FieldValues_191(b_1424))); };
}
class Arithmetic_19_Concept
{
    constructor(self) { this.Self = self; };
    static Add_1428 = function (self_1426, other_1427) { return Add_169(FieldValues_191(self_1426), FieldValues_191(other_1427)); };
    static Negative_1430 = function (self_1429) { return Negative_158(FieldValues_191(self_1429)); };
    static Reciprocal_1432 = function (self_1431) { return Reciprocal_160(FieldValues_191(self_1431)); };
    static Multiply_1435 = function (self_1433, other_1434) { return Add_169(FieldValues_191(self_1433), FieldValues_191(other_1434)); };
    static Divide_1438 = function (self_1436, other_1437) { return Divide_175(FieldValues_191(self_1436), FieldValues_191(other_1437)); };
    static Modulo_1441 = function (self_1439, other_1440) { return Modulo_177(FieldValues_191(self_1439), FieldValues_191(other_1440)); };
}
class ScalarArithmetic_20_Concept
{
    constructor(self) { this.Self = self; };
    static Add_1445 = function (self_1443, scalar_1444) { return Add_169(FieldValues_191(self_1443), scalar_1444); };
    static Subtract_1448 = function (self_1446, scalar_1447) { return Add_169(self_1446, Negative_158(scalar_1447)); };
    static Multiply_1451 = function (self_1449, scalar_1450) { return Multiply_173(FieldValues_191(self_1449), scalar_1450); };
    static Divide_1454 = function (self_1452, scalar_1453) { return Multiply_173(self_1452, Reciprocal_160(scalar_1453)); };
    static Modulo_1457 = function (self_1455, scalar_1456) { return Modulo_177(FieldValues_191(self_1455), scalar_1456); };
}
class Boolean_21_Concept
{
    constructor(self) { this.Self = self; };
    static And_1460 = function (a_1458, b_1459) { return And_179(FieldValues_191(a_1458), FieldValues_191(b_1459)); };
    static Or_1463 = function (a_1461, b_1462) { return Or_181(FieldValues_191(a_1461), FieldValues_191(b_1462)); };
    static Not_1465 = function (a_1464) { return Not_183(FieldValues_191(a_1464)); };
}
class Value_22_Concept
{
    constructor(self) { this.Self = self; };
    static Type_1466 = function () { return intrinsic_0; };
    static FieldTypes_1467 = function () { return intrinsic_0; };
    static FieldNames_1468 = function () { return intrinsic_0; };
    static FieldValues_1470 = function (self_1469) { return intrinsic_0; };
    static Zero_1471 = function () { return Zero_193(FieldTypes_187); };
    static One_1472 = function () { return One_195(FieldTypes_187); };
    static Default_1473 = function () { return Default_197(FieldTypes_187); };
    static MinValue_1474 = function () { return MinValue_199(FieldTypes_187); };
    static MaxValue_1475 = function () { return MaxValue_201(FieldTypes_187); };
    static ToString_1477 = function (x_1476) { return JoinStrings_1335(FieldValues_191, ","); };
}
class Interval_23_Concept
{
    constructor(self) { this.Self = self; };
    static Min_1480 = function (x_1479) { return null; };
    static Max_1482 = function (x_1481) { return null; };
}
class Array_24_Concept
{
    constructor(self) { this.Self = self; };
    static Count_1485 = function (xs_1484) { return null; };
    static At_1488 = function (xs_1486, n_1487) { return null; };
}
class Integer_25_Type
{
    constructor(Value_214)
    {
        // field initialization 
        this.Value_214 = Value_214;
        this.Type_1466 = Integer_25_Type.Value_22_Concept.Type_1466;
        this.FieldTypes_1467 = Integer_25_Type.Value_22_Concept.FieldTypes_1467;
        this.FieldNames_1468 = Integer_25_Type.Value_22_Concept.FieldNames_1468;
        this.FieldValues_1470 = Integer_25_Type.Value_22_Concept.FieldValues_1470;
        this.Zero_1471 = Integer_25_Type.Value_22_Concept.Zero_1471;
        this.One_1472 = Integer_25_Type.Value_22_Concept.One_1472;
        this.Default_1473 = Integer_25_Type.Value_22_Concept.Default_1473;
        this.MinValue_1474 = Integer_25_Type.Value_22_Concept.MinValue_1474;
        this.MaxValue_1475 = Integer_25_Type.Value_22_Concept.MaxValue_1475;
        this.ToString_1477 = Integer_25_Type.Value_22_Concept.ToString_1477;
        this.Add_1428 = Integer_25_Type.Arithmetic_19_Concept.Add_1428;
        this.Negative_1430 = Integer_25_Type.Arithmetic_19_Concept.Negative_1430;
        this.Reciprocal_1432 = Integer_25_Type.Arithmetic_19_Concept.Reciprocal_1432;
        this.Multiply_1435 = Integer_25_Type.Arithmetic_19_Concept.Multiply_1435;
        this.Divide_1438 = Integer_25_Type.Arithmetic_19_Concept.Divide_1438;
        this.Modulo_1441 = Integer_25_Type.Arithmetic_19_Concept.Modulo_1441;
        this.Equals_1425 = Integer_25_Type.Equatable_18_Concept.Equals_1425;
        this.Compare_1422 = Integer_25_Type.Comparable_17_Concept.Compare_1422;
        this.Magnitude_1419 = Integer_25_Type.Magnitude_16_Concept.Magnitude_1419;
    }
    // field accessors
    static Value_214 = function(self) { return self.Value_214; }
    // implemented concepts 
    static Value_22_Concept = new Value_22_Concept(Integer_25_Type);
    static Arithmetic_19_Concept = new Arithmetic_19_Concept(Integer_25_Type);
    static Equatable_18_Concept = new Equatable_18_Concept(Integer_25_Type);
    static Comparable_17_Concept = new Comparable_17_Concept(Integer_25_Type);
    static Magnitude_16_Concept = new Magnitude_16_Concept(Integer_25_Type);
    static Numerical_15_Concept = new Numerical_15_Concept(Integer_25_Type);
    static Implements = [Value_22_Concept,Arithmetic_19_Concept,Equatable_18_Concept,Comparable_17_Concept,Magnitude_16_Concept,Numerical_15_Concept];
}
class Count_26_Type
{
    constructor(Value_218)
    {
        // field initialization 
        this.Value_218 = Value_218;
        this.Type_1466 = Count_26_Type.Value_22_Concept.Type_1466;
        this.FieldTypes_1467 = Count_26_Type.Value_22_Concept.FieldTypes_1467;
        this.FieldNames_1468 = Count_26_Type.Value_22_Concept.FieldNames_1468;
        this.FieldValues_1470 = Count_26_Type.Value_22_Concept.FieldValues_1470;
        this.Zero_1471 = Count_26_Type.Value_22_Concept.Zero_1471;
        this.One_1472 = Count_26_Type.Value_22_Concept.One_1472;
        this.Default_1473 = Count_26_Type.Value_22_Concept.Default_1473;
        this.MinValue_1474 = Count_26_Type.Value_22_Concept.MinValue_1474;
        this.MaxValue_1475 = Count_26_Type.Value_22_Concept.MaxValue_1475;
        this.ToString_1477 = Count_26_Type.Value_22_Concept.ToString_1477;
        this.Add_1428 = Count_26_Type.Arithmetic_19_Concept.Add_1428;
        this.Negative_1430 = Count_26_Type.Arithmetic_19_Concept.Negative_1430;
        this.Reciprocal_1432 = Count_26_Type.Arithmetic_19_Concept.Reciprocal_1432;
        this.Multiply_1435 = Count_26_Type.Arithmetic_19_Concept.Multiply_1435;
        this.Divide_1438 = Count_26_Type.Arithmetic_19_Concept.Divide_1438;
        this.Modulo_1441 = Count_26_Type.Arithmetic_19_Concept.Modulo_1441;
        this.Equals_1425 = Count_26_Type.Equatable_18_Concept.Equals_1425;
        this.Compare_1422 = Count_26_Type.Comparable_17_Concept.Compare_1422;
        this.Magnitude_1419 = Count_26_Type.Magnitude_16_Concept.Magnitude_1419;
    }
    // field accessors
    static Value_218 = function(self) { return self.Value_218; }
    // implemented concepts 
    static Value_22_Concept = new Value_22_Concept(Count_26_Type);
    static Arithmetic_19_Concept = new Arithmetic_19_Concept(Count_26_Type);
    static Equatable_18_Concept = new Equatable_18_Concept(Count_26_Type);
    static Comparable_17_Concept = new Comparable_17_Concept(Count_26_Type);
    static Magnitude_16_Concept = new Magnitude_16_Concept(Count_26_Type);
    static Numerical_15_Concept = new Numerical_15_Concept(Count_26_Type);
    static Implements = [Value_22_Concept,Arithmetic_19_Concept,Equatable_18_Concept,Comparable_17_Concept,Magnitude_16_Concept,Numerical_15_Concept];
}
class Index_27_Type
{
    constructor(Value_222)
    {
        // field initialization 
        this.Value_222 = Value_222;
        this.Type_1466 = Index_27_Type.Value_22_Concept.Type_1466;
        this.FieldTypes_1467 = Index_27_Type.Value_22_Concept.FieldTypes_1467;
        this.FieldNames_1468 = Index_27_Type.Value_22_Concept.FieldNames_1468;
        this.FieldValues_1470 = Index_27_Type.Value_22_Concept.FieldValues_1470;
        this.Zero_1471 = Index_27_Type.Value_22_Concept.Zero_1471;
        this.One_1472 = Index_27_Type.Value_22_Concept.One_1472;
        this.Default_1473 = Index_27_Type.Value_22_Concept.Default_1473;
        this.MinValue_1474 = Index_27_Type.Value_22_Concept.MinValue_1474;
        this.MaxValue_1475 = Index_27_Type.Value_22_Concept.MaxValue_1475;
        this.ToString_1477 = Index_27_Type.Value_22_Concept.ToString_1477;
    }
    // field accessors
    static Value_222 = function(self) { return self.Value_222; }
    // implemented concepts 
    static Value_22_Concept = new Value_22_Concept(Index_27_Type);
    static Implements = [Value_22_Concept];
}
class Number_28_Type
{
    constructor(Value_226)
    {
        // field initialization 
        this.Value_226 = Value_226;
        this.Type_1466 = Number_28_Type.Value_22_Concept.Type_1466;
        this.FieldTypes_1467 = Number_28_Type.Value_22_Concept.FieldTypes_1467;
        this.FieldNames_1468 = Number_28_Type.Value_22_Concept.FieldNames_1468;
        this.FieldValues_1470 = Number_28_Type.Value_22_Concept.FieldValues_1470;
        this.Zero_1471 = Number_28_Type.Value_22_Concept.Zero_1471;
        this.One_1472 = Number_28_Type.Value_22_Concept.One_1472;
        this.Default_1473 = Number_28_Type.Value_22_Concept.Default_1473;
        this.MinValue_1474 = Number_28_Type.Value_22_Concept.MinValue_1474;
        this.MaxValue_1475 = Number_28_Type.Value_22_Concept.MaxValue_1475;
        this.ToString_1477 = Number_28_Type.Value_22_Concept.ToString_1477;
        this.Add_1428 = Number_28_Type.Arithmetic_19_Concept.Add_1428;
        this.Negative_1430 = Number_28_Type.Arithmetic_19_Concept.Negative_1430;
        this.Reciprocal_1432 = Number_28_Type.Arithmetic_19_Concept.Reciprocal_1432;
        this.Multiply_1435 = Number_28_Type.Arithmetic_19_Concept.Multiply_1435;
        this.Divide_1438 = Number_28_Type.Arithmetic_19_Concept.Divide_1438;
        this.Modulo_1441 = Number_28_Type.Arithmetic_19_Concept.Modulo_1441;
        this.Equals_1425 = Number_28_Type.Equatable_18_Concept.Equals_1425;
        this.Compare_1422 = Number_28_Type.Comparable_17_Concept.Compare_1422;
        this.Magnitude_1419 = Number_28_Type.Magnitude_16_Concept.Magnitude_1419;
    }
    // field accessors
    static Value_226 = function(self) { return self.Value_226; }
    // implemented concepts 
    static Value_22_Concept = new Value_22_Concept(Number_28_Type);
    static Arithmetic_19_Concept = new Arithmetic_19_Concept(Number_28_Type);
    static Equatable_18_Concept = new Equatable_18_Concept(Number_28_Type);
    static Comparable_17_Concept = new Comparable_17_Concept(Number_28_Type);
    static Magnitude_16_Concept = new Magnitude_16_Concept(Number_28_Type);
    static Numerical_15_Concept = new Numerical_15_Concept(Number_28_Type);
    static Implements = [Value_22_Concept,Arithmetic_19_Concept,Equatable_18_Concept,Comparable_17_Concept,Magnitude_16_Concept,Numerical_15_Concept];
}
class Unit_29_Type
{
    constructor(Value_230)
    {
        // field initialization 
        this.Value_230 = Value_230;
        this.Type_1466 = Unit_29_Type.Value_22_Concept.Type_1466;
        this.FieldTypes_1467 = Unit_29_Type.Value_22_Concept.FieldTypes_1467;
        this.FieldNames_1468 = Unit_29_Type.Value_22_Concept.FieldNames_1468;
        this.FieldValues_1470 = Unit_29_Type.Value_22_Concept.FieldValues_1470;
        this.Zero_1471 = Unit_29_Type.Value_22_Concept.Zero_1471;
        this.One_1472 = Unit_29_Type.Value_22_Concept.One_1472;
        this.Default_1473 = Unit_29_Type.Value_22_Concept.Default_1473;
        this.MinValue_1474 = Unit_29_Type.Value_22_Concept.MinValue_1474;
        this.MaxValue_1475 = Unit_29_Type.Value_22_Concept.MaxValue_1475;
        this.ToString_1477 = Unit_29_Type.Value_22_Concept.ToString_1477;
        this.Add_1428 = Unit_29_Type.Arithmetic_19_Concept.Add_1428;
        this.Negative_1430 = Unit_29_Type.Arithmetic_19_Concept.Negative_1430;
        this.Reciprocal_1432 = Unit_29_Type.Arithmetic_19_Concept.Reciprocal_1432;
        this.Multiply_1435 = Unit_29_Type.Arithmetic_19_Concept.Multiply_1435;
        this.Divide_1438 = Unit_29_Type.Arithmetic_19_Concept.Divide_1438;
        this.Modulo_1441 = Unit_29_Type.Arithmetic_19_Concept.Modulo_1441;
        this.Equals_1425 = Unit_29_Type.Equatable_18_Concept.Equals_1425;
        this.Compare_1422 = Unit_29_Type.Comparable_17_Concept.Compare_1422;
        this.Magnitude_1419 = Unit_29_Type.Magnitude_16_Concept.Magnitude_1419;
    }
    // field accessors
    static Value_230 = function(self) { return self.Value_230; }
    // implemented concepts 
    static Value_22_Concept = new Value_22_Concept(Unit_29_Type);
    static Arithmetic_19_Concept = new Arithmetic_19_Concept(Unit_29_Type);
    static Equatable_18_Concept = new Equatable_18_Concept(Unit_29_Type);
    static Comparable_17_Concept = new Comparable_17_Concept(Unit_29_Type);
    static Magnitude_16_Concept = new Magnitude_16_Concept(Unit_29_Type);
    static Numerical_15_Concept = new Numerical_15_Concept(Unit_29_Type);
    static Implements = [Value_22_Concept,Arithmetic_19_Concept,Equatable_18_Concept,Comparable_17_Concept,Magnitude_16_Concept,Numerical_15_Concept];
}
class Percent_30_Type
{
    constructor(Value_234)
    {
        // field initialization 
        this.Value_234 = Value_234;
        this.Type_1466 = Percent_30_Type.Value_22_Concept.Type_1466;
        this.FieldTypes_1467 = Percent_30_Type.Value_22_Concept.FieldTypes_1467;
        this.FieldNames_1468 = Percent_30_Type.Value_22_Concept.FieldNames_1468;
        this.FieldValues_1470 = Percent_30_Type.Value_22_Concept.FieldValues_1470;
        this.Zero_1471 = Percent_30_Type.Value_22_Concept.Zero_1471;
        this.One_1472 = Percent_30_Type.Value_22_Concept.One_1472;
        this.Default_1473 = Percent_30_Type.Value_22_Concept.Default_1473;
        this.MinValue_1474 = Percent_30_Type.Value_22_Concept.MinValue_1474;
        this.MaxValue_1475 = Percent_30_Type.Value_22_Concept.MaxValue_1475;
        this.ToString_1477 = Percent_30_Type.Value_22_Concept.ToString_1477;
        this.Add_1428 = Percent_30_Type.Arithmetic_19_Concept.Add_1428;
        this.Negative_1430 = Percent_30_Type.Arithmetic_19_Concept.Negative_1430;
        this.Reciprocal_1432 = Percent_30_Type.Arithmetic_19_Concept.Reciprocal_1432;
        this.Multiply_1435 = Percent_30_Type.Arithmetic_19_Concept.Multiply_1435;
        this.Divide_1438 = Percent_30_Type.Arithmetic_19_Concept.Divide_1438;
        this.Modulo_1441 = Percent_30_Type.Arithmetic_19_Concept.Modulo_1441;
        this.Equals_1425 = Percent_30_Type.Equatable_18_Concept.Equals_1425;
        this.Compare_1422 = Percent_30_Type.Comparable_17_Concept.Compare_1422;
        this.Magnitude_1419 = Percent_30_Type.Magnitude_16_Concept.Magnitude_1419;
    }
    // field accessors
    static Value_234 = function(self) { return self.Value_234; }
    // implemented concepts 
    static Value_22_Concept = new Value_22_Concept(Percent_30_Type);
    static Arithmetic_19_Concept = new Arithmetic_19_Concept(Percent_30_Type);
    static Equatable_18_Concept = new Equatable_18_Concept(Percent_30_Type);
    static Comparable_17_Concept = new Comparable_17_Concept(Percent_30_Type);
    static Magnitude_16_Concept = new Magnitude_16_Concept(Percent_30_Type);
    static Numerical_15_Concept = new Numerical_15_Concept(Percent_30_Type);
    static Implements = [Value_22_Concept,Arithmetic_19_Concept,Equatable_18_Concept,Comparable_17_Concept,Magnitude_16_Concept,Numerical_15_Concept];
}
class Quaternion_31_Type
{
    constructor(X_238, Y_242, Z_246, W_250)
    {
        // field initialization 
        this.X_238 = X_238;
        this.Y_242 = Y_242;
        this.Z_246 = Z_246;
        this.W_250 = W_250;
        this.Type_1466 = Quaternion_31_Type.Value_22_Concept.Type_1466;
        this.FieldTypes_1467 = Quaternion_31_Type.Value_22_Concept.FieldTypes_1467;
        this.FieldNames_1468 = Quaternion_31_Type.Value_22_Concept.FieldNames_1468;
        this.FieldValues_1470 = Quaternion_31_Type.Value_22_Concept.FieldValues_1470;
        this.Zero_1471 = Quaternion_31_Type.Value_22_Concept.Zero_1471;
        this.One_1472 = Quaternion_31_Type.Value_22_Concept.One_1472;
        this.Default_1473 = Quaternion_31_Type.Value_22_Concept.Default_1473;
        this.MinValue_1474 = Quaternion_31_Type.Value_22_Concept.MinValue_1474;
        this.MaxValue_1475 = Quaternion_31_Type.Value_22_Concept.MaxValue_1475;
        this.ToString_1477 = Quaternion_31_Type.Value_22_Concept.ToString_1477;
    }
    // field accessors
    static X_238 = function(self) { return self.X_238; }
    static Y_242 = function(self) { return self.Y_242; }
    static Z_246 = function(self) { return self.Z_246; }
    static W_250 = function(self) { return self.W_250; }
    // implemented concepts 
    static Value_22_Concept = new Value_22_Concept(Quaternion_31_Type);
    static Implements = [Value_22_Concept];
}
class Unit2D_32_Type
{
    constructor(X_254, Y_258)
    {
        // field initialization 
        this.X_254 = X_254;
        this.Y_258 = Y_258;
        this.Type_1466 = Unit2D_32_Type.Value_22_Concept.Type_1466;
        this.FieldTypes_1467 = Unit2D_32_Type.Value_22_Concept.FieldTypes_1467;
        this.FieldNames_1468 = Unit2D_32_Type.Value_22_Concept.FieldNames_1468;
        this.FieldValues_1470 = Unit2D_32_Type.Value_22_Concept.FieldValues_1470;
        this.Zero_1471 = Unit2D_32_Type.Value_22_Concept.Zero_1471;
        this.One_1472 = Unit2D_32_Type.Value_22_Concept.One_1472;
        this.Default_1473 = Unit2D_32_Type.Value_22_Concept.Default_1473;
        this.MinValue_1474 = Unit2D_32_Type.Value_22_Concept.MinValue_1474;
        this.MaxValue_1475 = Unit2D_32_Type.Value_22_Concept.MaxValue_1475;
        this.ToString_1477 = Unit2D_32_Type.Value_22_Concept.ToString_1477;
    }
    // field accessors
    static X_254 = function(self) { return self.X_254; }
    static Y_258 = function(self) { return self.Y_258; }
    // implemented concepts 
    static Value_22_Concept = new Value_22_Concept(Unit2D_32_Type);
    static Implements = [Value_22_Concept];
}
class Unit3D_33_Type
{
    constructor(X_262, Y_266, Z_270)
    {
        // field initialization 
        this.X_262 = X_262;
        this.Y_266 = Y_266;
        this.Z_270 = Z_270;
        this.Type_1466 = Unit3D_33_Type.Value_22_Concept.Type_1466;
        this.FieldTypes_1467 = Unit3D_33_Type.Value_22_Concept.FieldTypes_1467;
        this.FieldNames_1468 = Unit3D_33_Type.Value_22_Concept.FieldNames_1468;
        this.FieldValues_1470 = Unit3D_33_Type.Value_22_Concept.FieldValues_1470;
        this.Zero_1471 = Unit3D_33_Type.Value_22_Concept.Zero_1471;
        this.One_1472 = Unit3D_33_Type.Value_22_Concept.One_1472;
        this.Default_1473 = Unit3D_33_Type.Value_22_Concept.Default_1473;
        this.MinValue_1474 = Unit3D_33_Type.Value_22_Concept.MinValue_1474;
        this.MaxValue_1475 = Unit3D_33_Type.Value_22_Concept.MaxValue_1475;
        this.ToString_1477 = Unit3D_33_Type.Value_22_Concept.ToString_1477;
    }
    // field accessors
    static X_262 = function(self) { return self.X_262; }
    static Y_266 = function(self) { return self.Y_266; }
    static Z_270 = function(self) { return self.Z_270; }
    // implemented concepts 
    static Value_22_Concept = new Value_22_Concept(Unit3D_33_Type);
    static Implements = [Value_22_Concept];
}
class Direction3D_34_Type
{
    constructor(Value_274)
    {
        // field initialization 
        this.Value_274 = Value_274;
        this.Type_1466 = Direction3D_34_Type.Value_22_Concept.Type_1466;
        this.FieldTypes_1467 = Direction3D_34_Type.Value_22_Concept.FieldTypes_1467;
        this.FieldNames_1468 = Direction3D_34_Type.Value_22_Concept.FieldNames_1468;
        this.FieldValues_1470 = Direction3D_34_Type.Value_22_Concept.FieldValues_1470;
        this.Zero_1471 = Direction3D_34_Type.Value_22_Concept.Zero_1471;
        this.One_1472 = Direction3D_34_Type.Value_22_Concept.One_1472;
        this.Default_1473 = Direction3D_34_Type.Value_22_Concept.Default_1473;
        this.MinValue_1474 = Direction3D_34_Type.Value_22_Concept.MinValue_1474;
        this.MaxValue_1475 = Direction3D_34_Type.Value_22_Concept.MaxValue_1475;
        this.ToString_1477 = Direction3D_34_Type.Value_22_Concept.ToString_1477;
    }
    // field accessors
    static Value_274 = function(self) { return self.Value_274; }
    // implemented concepts 
    static Value_22_Concept = new Value_22_Concept(Direction3D_34_Type);
    static Implements = [Value_22_Concept];
}
class AxisAngle_35_Type
{
    constructor(Axis_278, Angle_282)
    {
        // field initialization 
        this.Axis_278 = Axis_278;
        this.Angle_282 = Angle_282;
        this.Type_1466 = AxisAngle_35_Type.Value_22_Concept.Type_1466;
        this.FieldTypes_1467 = AxisAngle_35_Type.Value_22_Concept.FieldTypes_1467;
        this.FieldNames_1468 = AxisAngle_35_Type.Value_22_Concept.FieldNames_1468;
        this.FieldValues_1470 = AxisAngle_35_Type.Value_22_Concept.FieldValues_1470;
        this.Zero_1471 = AxisAngle_35_Type.Value_22_Concept.Zero_1471;
        this.One_1472 = AxisAngle_35_Type.Value_22_Concept.One_1472;
        this.Default_1473 = AxisAngle_35_Type.Value_22_Concept.Default_1473;
        this.MinValue_1474 = AxisAngle_35_Type.Value_22_Concept.MinValue_1474;
        this.MaxValue_1475 = AxisAngle_35_Type.Value_22_Concept.MaxValue_1475;
        this.ToString_1477 = AxisAngle_35_Type.Value_22_Concept.ToString_1477;
    }
    // field accessors
    static Axis_278 = function(self) { return self.Axis_278; }
    static Angle_282 = function(self) { return self.Angle_282; }
    // implemented concepts 
    static Value_22_Concept = new Value_22_Concept(AxisAngle_35_Type);
    static Implements = [Value_22_Concept];
}
class EulerAngles_36_Type
{
    constructor(Yaw_286, Pitch_290, Roll_294)
    {
        // field initialization 
        this.Yaw_286 = Yaw_286;
        this.Pitch_290 = Pitch_290;
        this.Roll_294 = Roll_294;
        this.Type_1466 = EulerAngles_36_Type.Value_22_Concept.Type_1466;
        this.FieldTypes_1467 = EulerAngles_36_Type.Value_22_Concept.FieldTypes_1467;
        this.FieldNames_1468 = EulerAngles_36_Type.Value_22_Concept.FieldNames_1468;
        this.FieldValues_1470 = EulerAngles_36_Type.Value_22_Concept.FieldValues_1470;
        this.Zero_1471 = EulerAngles_36_Type.Value_22_Concept.Zero_1471;
        this.One_1472 = EulerAngles_36_Type.Value_22_Concept.One_1472;
        this.Default_1473 = EulerAngles_36_Type.Value_22_Concept.Default_1473;
        this.MinValue_1474 = EulerAngles_36_Type.Value_22_Concept.MinValue_1474;
        this.MaxValue_1475 = EulerAngles_36_Type.Value_22_Concept.MaxValue_1475;
        this.ToString_1477 = EulerAngles_36_Type.Value_22_Concept.ToString_1477;
    }
    // field accessors
    static Yaw_286 = function(self) { return self.Yaw_286; }
    static Pitch_290 = function(self) { return self.Pitch_290; }
    static Roll_294 = function(self) { return self.Roll_294; }
    // implemented concepts 
    static Value_22_Concept = new Value_22_Concept(EulerAngles_36_Type);
    static Implements = [Value_22_Concept];
}
class Rotation3D_37_Type
{
    constructor(Quaternion_298)
    {
        // field initialization 
        this.Quaternion_298 = Quaternion_298;
        this.Type_1466 = Rotation3D_37_Type.Value_22_Concept.Type_1466;
        this.FieldTypes_1467 = Rotation3D_37_Type.Value_22_Concept.FieldTypes_1467;
        this.FieldNames_1468 = Rotation3D_37_Type.Value_22_Concept.FieldNames_1468;
        this.FieldValues_1470 = Rotation3D_37_Type.Value_22_Concept.FieldValues_1470;
        this.Zero_1471 = Rotation3D_37_Type.Value_22_Concept.Zero_1471;
        this.One_1472 = Rotation3D_37_Type.Value_22_Concept.One_1472;
        this.Default_1473 = Rotation3D_37_Type.Value_22_Concept.Default_1473;
        this.MinValue_1474 = Rotation3D_37_Type.Value_22_Concept.MinValue_1474;
        this.MaxValue_1475 = Rotation3D_37_Type.Value_22_Concept.MaxValue_1475;
        this.ToString_1477 = Rotation3D_37_Type.Value_22_Concept.ToString_1477;
    }
    // field accessors
    static Quaternion_298 = function(self) { return self.Quaternion_298; }
    // implemented concepts 
    static Value_22_Concept = new Value_22_Concept(Rotation3D_37_Type);
    static Implements = [Value_22_Concept];
}
class Vector2D_38_Type
{
    constructor(X_302, Y_306)
    {
        // field initialization 
        this.X_302 = X_302;
        this.Y_306 = Y_306;
        this.Count_1485 = Vector2D_38_Type.Array_24_Concept.Count_1485;
        this.At_1488 = Vector2D_38_Type.Array_24_Concept.At_1488;
        this.Type_1466 = Vector2D_38_Type.Value_22_Concept.Type_1466;
        this.FieldTypes_1467 = Vector2D_38_Type.Value_22_Concept.FieldTypes_1467;
        this.FieldNames_1468 = Vector2D_38_Type.Value_22_Concept.FieldNames_1468;
        this.FieldValues_1470 = Vector2D_38_Type.Value_22_Concept.FieldValues_1470;
        this.Zero_1471 = Vector2D_38_Type.Value_22_Concept.Zero_1471;
        this.One_1472 = Vector2D_38_Type.Value_22_Concept.One_1472;
        this.Default_1473 = Vector2D_38_Type.Value_22_Concept.Default_1473;
        this.MinValue_1474 = Vector2D_38_Type.Value_22_Concept.MinValue_1474;
        this.MaxValue_1475 = Vector2D_38_Type.Value_22_Concept.MaxValue_1475;
        this.ToString_1477 = Vector2D_38_Type.Value_22_Concept.ToString_1477;
        this.Add_1428 = Vector2D_38_Type.Arithmetic_19_Concept.Add_1428;
        this.Negative_1430 = Vector2D_38_Type.Arithmetic_19_Concept.Negative_1430;
        this.Reciprocal_1432 = Vector2D_38_Type.Arithmetic_19_Concept.Reciprocal_1432;
        this.Multiply_1435 = Vector2D_38_Type.Arithmetic_19_Concept.Multiply_1435;
        this.Divide_1438 = Vector2D_38_Type.Arithmetic_19_Concept.Divide_1438;
        this.Modulo_1441 = Vector2D_38_Type.Arithmetic_19_Concept.Modulo_1441;
        this.Equals_1425 = Vector2D_38_Type.Equatable_18_Concept.Equals_1425;
        this.Compare_1422 = Vector2D_38_Type.Comparable_17_Concept.Compare_1422;
        this.Magnitude_1419 = Vector2D_38_Type.Magnitude_16_Concept.Magnitude_1419;
        this.Count_1412 = Vector2D_38_Type.Vector_13_Concept.Count_1412;
        this.At_1415 = Vector2D_38_Type.Vector_13_Concept.At_1415;
    }
    // field accessors
    static X_302 = function(self) { return self.X_302; }
    static Y_306 = function(self) { return self.Y_306; }
    // implemented concepts 
    static Array_24_Concept = new Array_24_Concept(Vector2D_38_Type);
    static Value_22_Concept = new Value_22_Concept(Vector2D_38_Type);
    static Arithmetic_19_Concept = new Arithmetic_19_Concept(Vector2D_38_Type);
    static Equatable_18_Concept = new Equatable_18_Concept(Vector2D_38_Type);
    static Comparable_17_Concept = new Comparable_17_Concept(Vector2D_38_Type);
    static Magnitude_16_Concept = new Magnitude_16_Concept(Vector2D_38_Type);
    static Numerical_15_Concept = new Numerical_15_Concept(Vector2D_38_Type);
    static Vector_13_Concept = new Vector_13_Concept(Vector2D_38_Type);
    static Implements = [Array_24_Concept,Value_22_Concept,Arithmetic_19_Concept,Equatable_18_Concept,Comparable_17_Concept,Magnitude_16_Concept,Numerical_15_Concept,Vector_13_Concept];
}
class Vector3D_39_Type
{
    constructor(X_310, Y_314, Z_318)
    {
        // field initialization 
        this.X_310 = X_310;
        this.Y_314 = Y_314;
        this.Z_318 = Z_318;
        this.Count_1485 = Vector3D_39_Type.Array_24_Concept.Count_1485;
        this.At_1488 = Vector3D_39_Type.Array_24_Concept.At_1488;
        this.Type_1466 = Vector3D_39_Type.Value_22_Concept.Type_1466;
        this.FieldTypes_1467 = Vector3D_39_Type.Value_22_Concept.FieldTypes_1467;
        this.FieldNames_1468 = Vector3D_39_Type.Value_22_Concept.FieldNames_1468;
        this.FieldValues_1470 = Vector3D_39_Type.Value_22_Concept.FieldValues_1470;
        this.Zero_1471 = Vector3D_39_Type.Value_22_Concept.Zero_1471;
        this.One_1472 = Vector3D_39_Type.Value_22_Concept.One_1472;
        this.Default_1473 = Vector3D_39_Type.Value_22_Concept.Default_1473;
        this.MinValue_1474 = Vector3D_39_Type.Value_22_Concept.MinValue_1474;
        this.MaxValue_1475 = Vector3D_39_Type.Value_22_Concept.MaxValue_1475;
        this.ToString_1477 = Vector3D_39_Type.Value_22_Concept.ToString_1477;
        this.Add_1428 = Vector3D_39_Type.Arithmetic_19_Concept.Add_1428;
        this.Negative_1430 = Vector3D_39_Type.Arithmetic_19_Concept.Negative_1430;
        this.Reciprocal_1432 = Vector3D_39_Type.Arithmetic_19_Concept.Reciprocal_1432;
        this.Multiply_1435 = Vector3D_39_Type.Arithmetic_19_Concept.Multiply_1435;
        this.Divide_1438 = Vector3D_39_Type.Arithmetic_19_Concept.Divide_1438;
        this.Modulo_1441 = Vector3D_39_Type.Arithmetic_19_Concept.Modulo_1441;
        this.Equals_1425 = Vector3D_39_Type.Equatable_18_Concept.Equals_1425;
        this.Compare_1422 = Vector3D_39_Type.Comparable_17_Concept.Compare_1422;
        this.Magnitude_1419 = Vector3D_39_Type.Magnitude_16_Concept.Magnitude_1419;
        this.Count_1412 = Vector3D_39_Type.Vector_13_Concept.Count_1412;
        this.At_1415 = Vector3D_39_Type.Vector_13_Concept.At_1415;
    }
    // field accessors
    static X_310 = function(self) { return self.X_310; }
    static Y_314 = function(self) { return self.Y_314; }
    static Z_318 = function(self) { return self.Z_318; }
    // implemented concepts 
    static Array_24_Concept = new Array_24_Concept(Vector3D_39_Type);
    static Value_22_Concept = new Value_22_Concept(Vector3D_39_Type);
    static Arithmetic_19_Concept = new Arithmetic_19_Concept(Vector3D_39_Type);
    static Equatable_18_Concept = new Equatable_18_Concept(Vector3D_39_Type);
    static Comparable_17_Concept = new Comparable_17_Concept(Vector3D_39_Type);
    static Magnitude_16_Concept = new Magnitude_16_Concept(Vector3D_39_Type);
    static Numerical_15_Concept = new Numerical_15_Concept(Vector3D_39_Type);
    static Vector_13_Concept = new Vector_13_Concept(Vector3D_39_Type);
    static Implements = [Array_24_Concept,Value_22_Concept,Arithmetic_19_Concept,Equatable_18_Concept,Comparable_17_Concept,Magnitude_16_Concept,Numerical_15_Concept,Vector_13_Concept];
}
class Vector4D_40_Type
{
    constructor(X_322, Y_326, Z_330, W_334)
    {
        // field initialization 
        this.X_322 = X_322;
        this.Y_326 = Y_326;
        this.Z_330 = Z_330;
        this.W_334 = W_334;
        this.Count_1485 = Vector4D_40_Type.Array_24_Concept.Count_1485;
        this.At_1488 = Vector4D_40_Type.Array_24_Concept.At_1488;
        this.Type_1466 = Vector4D_40_Type.Value_22_Concept.Type_1466;
        this.FieldTypes_1467 = Vector4D_40_Type.Value_22_Concept.FieldTypes_1467;
        this.FieldNames_1468 = Vector4D_40_Type.Value_22_Concept.FieldNames_1468;
        this.FieldValues_1470 = Vector4D_40_Type.Value_22_Concept.FieldValues_1470;
        this.Zero_1471 = Vector4D_40_Type.Value_22_Concept.Zero_1471;
        this.One_1472 = Vector4D_40_Type.Value_22_Concept.One_1472;
        this.Default_1473 = Vector4D_40_Type.Value_22_Concept.Default_1473;
        this.MinValue_1474 = Vector4D_40_Type.Value_22_Concept.MinValue_1474;
        this.MaxValue_1475 = Vector4D_40_Type.Value_22_Concept.MaxValue_1475;
        this.ToString_1477 = Vector4D_40_Type.Value_22_Concept.ToString_1477;
        this.Add_1428 = Vector4D_40_Type.Arithmetic_19_Concept.Add_1428;
        this.Negative_1430 = Vector4D_40_Type.Arithmetic_19_Concept.Negative_1430;
        this.Reciprocal_1432 = Vector4D_40_Type.Arithmetic_19_Concept.Reciprocal_1432;
        this.Multiply_1435 = Vector4D_40_Type.Arithmetic_19_Concept.Multiply_1435;
        this.Divide_1438 = Vector4D_40_Type.Arithmetic_19_Concept.Divide_1438;
        this.Modulo_1441 = Vector4D_40_Type.Arithmetic_19_Concept.Modulo_1441;
        this.Equals_1425 = Vector4D_40_Type.Equatable_18_Concept.Equals_1425;
        this.Compare_1422 = Vector4D_40_Type.Comparable_17_Concept.Compare_1422;
        this.Magnitude_1419 = Vector4D_40_Type.Magnitude_16_Concept.Magnitude_1419;
        this.Count_1412 = Vector4D_40_Type.Vector_13_Concept.Count_1412;
        this.At_1415 = Vector4D_40_Type.Vector_13_Concept.At_1415;
    }
    // field accessors
    static X_322 = function(self) { return self.X_322; }
    static Y_326 = function(self) { return self.Y_326; }
    static Z_330 = function(self) { return self.Z_330; }
    static W_334 = function(self) { return self.W_334; }
    // implemented concepts 
    static Array_24_Concept = new Array_24_Concept(Vector4D_40_Type);
    static Value_22_Concept = new Value_22_Concept(Vector4D_40_Type);
    static Arithmetic_19_Concept = new Arithmetic_19_Concept(Vector4D_40_Type);
    static Equatable_18_Concept = new Equatable_18_Concept(Vector4D_40_Type);
    static Comparable_17_Concept = new Comparable_17_Concept(Vector4D_40_Type);
    static Magnitude_16_Concept = new Magnitude_16_Concept(Vector4D_40_Type);
    static Numerical_15_Concept = new Numerical_15_Concept(Vector4D_40_Type);
    static Vector_13_Concept = new Vector_13_Concept(Vector4D_40_Type);
    static Implements = [Array_24_Concept,Value_22_Concept,Arithmetic_19_Concept,Equatable_18_Concept,Comparable_17_Concept,Magnitude_16_Concept,Numerical_15_Concept,Vector_13_Concept];
}
class Orientation3D_41_Type
{
    constructor(Value_338)
    {
        // field initialization 
        this.Value_338 = Value_338;
        this.Type_1466 = Orientation3D_41_Type.Value_22_Concept.Type_1466;
        this.FieldTypes_1467 = Orientation3D_41_Type.Value_22_Concept.FieldTypes_1467;
        this.FieldNames_1468 = Orientation3D_41_Type.Value_22_Concept.FieldNames_1468;
        this.FieldValues_1470 = Orientation3D_41_Type.Value_22_Concept.FieldValues_1470;
        this.Zero_1471 = Orientation3D_41_Type.Value_22_Concept.Zero_1471;
        this.One_1472 = Orientation3D_41_Type.Value_22_Concept.One_1472;
        this.Default_1473 = Orientation3D_41_Type.Value_22_Concept.Default_1473;
        this.MinValue_1474 = Orientation3D_41_Type.Value_22_Concept.MinValue_1474;
        this.MaxValue_1475 = Orientation3D_41_Type.Value_22_Concept.MaxValue_1475;
        this.ToString_1477 = Orientation3D_41_Type.Value_22_Concept.ToString_1477;
    }
    // field accessors
    static Value_338 = function(self) { return self.Value_338; }
    // implemented concepts 
    static Value_22_Concept = new Value_22_Concept(Orientation3D_41_Type);
    static Implements = [Value_22_Concept];
}
class Pose2D_42_Type
{
    constructor(Position_342, Orientation_346)
    {
        // field initialization 
        this.Position_342 = Position_342;
        this.Orientation_346 = Orientation_346;
        this.Type_1466 = Pose2D_42_Type.Value_22_Concept.Type_1466;
        this.FieldTypes_1467 = Pose2D_42_Type.Value_22_Concept.FieldTypes_1467;
        this.FieldNames_1468 = Pose2D_42_Type.Value_22_Concept.FieldNames_1468;
        this.FieldValues_1470 = Pose2D_42_Type.Value_22_Concept.FieldValues_1470;
        this.Zero_1471 = Pose2D_42_Type.Value_22_Concept.Zero_1471;
        this.One_1472 = Pose2D_42_Type.Value_22_Concept.One_1472;
        this.Default_1473 = Pose2D_42_Type.Value_22_Concept.Default_1473;
        this.MinValue_1474 = Pose2D_42_Type.Value_22_Concept.MinValue_1474;
        this.MaxValue_1475 = Pose2D_42_Type.Value_22_Concept.MaxValue_1475;
        this.ToString_1477 = Pose2D_42_Type.Value_22_Concept.ToString_1477;
    }
    // field accessors
    static Position_342 = function(self) { return self.Position_342; }
    static Orientation_346 = function(self) { return self.Orientation_346; }
    // implemented concepts 
    static Value_22_Concept = new Value_22_Concept(Pose2D_42_Type);
    static Implements = [Value_22_Concept];
}
class Pose3D_43_Type
{
    constructor(Position_350, Orientation_354)
    {
        // field initialization 
        this.Position_350 = Position_350;
        this.Orientation_354 = Orientation_354;
        this.Type_1466 = Pose3D_43_Type.Value_22_Concept.Type_1466;
        this.FieldTypes_1467 = Pose3D_43_Type.Value_22_Concept.FieldTypes_1467;
        this.FieldNames_1468 = Pose3D_43_Type.Value_22_Concept.FieldNames_1468;
        this.FieldValues_1470 = Pose3D_43_Type.Value_22_Concept.FieldValues_1470;
        this.Zero_1471 = Pose3D_43_Type.Value_22_Concept.Zero_1471;
        this.One_1472 = Pose3D_43_Type.Value_22_Concept.One_1472;
        this.Default_1473 = Pose3D_43_Type.Value_22_Concept.Default_1473;
        this.MinValue_1474 = Pose3D_43_Type.Value_22_Concept.MinValue_1474;
        this.MaxValue_1475 = Pose3D_43_Type.Value_22_Concept.MaxValue_1475;
        this.ToString_1477 = Pose3D_43_Type.Value_22_Concept.ToString_1477;
    }
    // field accessors
    static Position_350 = function(self) { return self.Position_350; }
    static Orientation_354 = function(self) { return self.Orientation_354; }
    // implemented concepts 
    static Value_22_Concept = new Value_22_Concept(Pose3D_43_Type);
    static Implements = [Value_22_Concept];
}
class Transform3D_44_Type
{
    constructor(Translation_358, Rotation_362, Scale_366)
    {
        // field initialization 
        this.Translation_358 = Translation_358;
        this.Rotation_362 = Rotation_362;
        this.Scale_366 = Scale_366;
        this.Type_1466 = Transform3D_44_Type.Value_22_Concept.Type_1466;
        this.FieldTypes_1467 = Transform3D_44_Type.Value_22_Concept.FieldTypes_1467;
        this.FieldNames_1468 = Transform3D_44_Type.Value_22_Concept.FieldNames_1468;
        this.FieldValues_1470 = Transform3D_44_Type.Value_22_Concept.FieldValues_1470;
        this.Zero_1471 = Transform3D_44_Type.Value_22_Concept.Zero_1471;
        this.One_1472 = Transform3D_44_Type.Value_22_Concept.One_1472;
        this.Default_1473 = Transform3D_44_Type.Value_22_Concept.Default_1473;
        this.MinValue_1474 = Transform3D_44_Type.Value_22_Concept.MinValue_1474;
        this.MaxValue_1475 = Transform3D_44_Type.Value_22_Concept.MaxValue_1475;
        this.ToString_1477 = Transform3D_44_Type.Value_22_Concept.ToString_1477;
    }
    // field accessors
    static Translation_358 = function(self) { return self.Translation_358; }
    static Rotation_362 = function(self) { return self.Rotation_362; }
    static Scale_366 = function(self) { return self.Scale_366; }
    // implemented concepts 
    static Value_22_Concept = new Value_22_Concept(Transform3D_44_Type);
    static Implements = [Value_22_Concept];
}
class Transform2D_45_Type
{
    constructor(Translation_370, Rotation_374, Scale_378)
    {
        // field initialization 
        this.Translation_370 = Translation_370;
        this.Rotation_374 = Rotation_374;
        this.Scale_378 = Scale_378;
        this.Type_1466 = Transform2D_45_Type.Value_22_Concept.Type_1466;
        this.FieldTypes_1467 = Transform2D_45_Type.Value_22_Concept.FieldTypes_1467;
        this.FieldNames_1468 = Transform2D_45_Type.Value_22_Concept.FieldNames_1468;
        this.FieldValues_1470 = Transform2D_45_Type.Value_22_Concept.FieldValues_1470;
        this.Zero_1471 = Transform2D_45_Type.Value_22_Concept.Zero_1471;
        this.One_1472 = Transform2D_45_Type.Value_22_Concept.One_1472;
        this.Default_1473 = Transform2D_45_Type.Value_22_Concept.Default_1473;
        this.MinValue_1474 = Transform2D_45_Type.Value_22_Concept.MinValue_1474;
        this.MaxValue_1475 = Transform2D_45_Type.Value_22_Concept.MaxValue_1475;
        this.ToString_1477 = Transform2D_45_Type.Value_22_Concept.ToString_1477;
    }
    // field accessors
    static Translation_370 = function(self) { return self.Translation_370; }
    static Rotation_374 = function(self) { return self.Rotation_374; }
    static Scale_378 = function(self) { return self.Scale_378; }
    // implemented concepts 
    static Value_22_Concept = new Value_22_Concept(Transform2D_45_Type);
    static Implements = [Value_22_Concept];
}
class AlignedBox2D_46_Type
{
    constructor(A_382, B_386)
    {
        // field initialization 
        this.A_382 = A_382;
        this.B_386 = B_386;
        this.Count_1485 = AlignedBox2D_46_Type.Array_24_Concept.Count_1485;
        this.At_1488 = AlignedBox2D_46_Type.Array_24_Concept.At_1488;
        this.Type_1466 = AlignedBox2D_46_Type.Value_22_Concept.Type_1466;
        this.FieldTypes_1467 = AlignedBox2D_46_Type.Value_22_Concept.FieldTypes_1467;
        this.FieldNames_1468 = AlignedBox2D_46_Type.Value_22_Concept.FieldNames_1468;
        this.FieldValues_1470 = AlignedBox2D_46_Type.Value_22_Concept.FieldValues_1470;
        this.Zero_1471 = AlignedBox2D_46_Type.Value_22_Concept.Zero_1471;
        this.One_1472 = AlignedBox2D_46_Type.Value_22_Concept.One_1472;
        this.Default_1473 = AlignedBox2D_46_Type.Value_22_Concept.Default_1473;
        this.MinValue_1474 = AlignedBox2D_46_Type.Value_22_Concept.MinValue_1474;
        this.MaxValue_1475 = AlignedBox2D_46_Type.Value_22_Concept.MaxValue_1475;
        this.ToString_1477 = AlignedBox2D_46_Type.Value_22_Concept.ToString_1477;
        this.Add_1428 = AlignedBox2D_46_Type.Arithmetic_19_Concept.Add_1428;
        this.Negative_1430 = AlignedBox2D_46_Type.Arithmetic_19_Concept.Negative_1430;
        this.Reciprocal_1432 = AlignedBox2D_46_Type.Arithmetic_19_Concept.Reciprocal_1432;
        this.Multiply_1435 = AlignedBox2D_46_Type.Arithmetic_19_Concept.Multiply_1435;
        this.Divide_1438 = AlignedBox2D_46_Type.Arithmetic_19_Concept.Divide_1438;
        this.Modulo_1441 = AlignedBox2D_46_Type.Arithmetic_19_Concept.Modulo_1441;
        this.Equals_1425 = AlignedBox2D_46_Type.Equatable_18_Concept.Equals_1425;
        this.Compare_1422 = AlignedBox2D_46_Type.Comparable_17_Concept.Compare_1422;
        this.Magnitude_1419 = AlignedBox2D_46_Type.Magnitude_16_Concept.Magnitude_1419;
        this.Count_1412 = AlignedBox2D_46_Type.Vector_13_Concept.Count_1412;
        this.At_1415 = AlignedBox2D_46_Type.Vector_13_Concept.At_1415;
        this.Min_1480 = AlignedBox2D_46_Type.Interval_23_Concept.Min_1480;
        this.Max_1482 = AlignedBox2D_46_Type.Interval_23_Concept.Max_1482;
    }
    // field accessors
    static A_382 = function(self) { return self.A_382; }
    static B_386 = function(self) { return self.B_386; }
    // implemented concepts 
    static Array_24_Concept = new Array_24_Concept(AlignedBox2D_46_Type);
    static Value_22_Concept = new Value_22_Concept(AlignedBox2D_46_Type);
    static Arithmetic_19_Concept = new Arithmetic_19_Concept(AlignedBox2D_46_Type);
    static Equatable_18_Concept = new Equatable_18_Concept(AlignedBox2D_46_Type);
    static Comparable_17_Concept = new Comparable_17_Concept(AlignedBox2D_46_Type);
    static Magnitude_16_Concept = new Magnitude_16_Concept(AlignedBox2D_46_Type);
    static Numerical_15_Concept = new Numerical_15_Concept(AlignedBox2D_46_Type);
    static Vector_13_Concept = new Vector_13_Concept(AlignedBox2D_46_Type);
    static Interval_23_Concept = new Interval_23_Concept(AlignedBox2D_46_Type);
    static Implements = [Array_24_Concept,Value_22_Concept,Arithmetic_19_Concept,Equatable_18_Concept,Comparable_17_Concept,Magnitude_16_Concept,Numerical_15_Concept,Vector_13_Concept,Interval_23_Concept];
}
class AlignedBox3D_47_Type
{
    constructor(A_390, B_394)
    {
        // field initialization 
        this.A_390 = A_390;
        this.B_394 = B_394;
        this.Count_1485 = AlignedBox3D_47_Type.Array_24_Concept.Count_1485;
        this.At_1488 = AlignedBox3D_47_Type.Array_24_Concept.At_1488;
        this.Type_1466 = AlignedBox3D_47_Type.Value_22_Concept.Type_1466;
        this.FieldTypes_1467 = AlignedBox3D_47_Type.Value_22_Concept.FieldTypes_1467;
        this.FieldNames_1468 = AlignedBox3D_47_Type.Value_22_Concept.FieldNames_1468;
        this.FieldValues_1470 = AlignedBox3D_47_Type.Value_22_Concept.FieldValues_1470;
        this.Zero_1471 = AlignedBox3D_47_Type.Value_22_Concept.Zero_1471;
        this.One_1472 = AlignedBox3D_47_Type.Value_22_Concept.One_1472;
        this.Default_1473 = AlignedBox3D_47_Type.Value_22_Concept.Default_1473;
        this.MinValue_1474 = AlignedBox3D_47_Type.Value_22_Concept.MinValue_1474;
        this.MaxValue_1475 = AlignedBox3D_47_Type.Value_22_Concept.MaxValue_1475;
        this.ToString_1477 = AlignedBox3D_47_Type.Value_22_Concept.ToString_1477;
        this.Add_1428 = AlignedBox3D_47_Type.Arithmetic_19_Concept.Add_1428;
        this.Negative_1430 = AlignedBox3D_47_Type.Arithmetic_19_Concept.Negative_1430;
        this.Reciprocal_1432 = AlignedBox3D_47_Type.Arithmetic_19_Concept.Reciprocal_1432;
        this.Multiply_1435 = AlignedBox3D_47_Type.Arithmetic_19_Concept.Multiply_1435;
        this.Divide_1438 = AlignedBox3D_47_Type.Arithmetic_19_Concept.Divide_1438;
        this.Modulo_1441 = AlignedBox3D_47_Type.Arithmetic_19_Concept.Modulo_1441;
        this.Equals_1425 = AlignedBox3D_47_Type.Equatable_18_Concept.Equals_1425;
        this.Compare_1422 = AlignedBox3D_47_Type.Comparable_17_Concept.Compare_1422;
        this.Magnitude_1419 = AlignedBox3D_47_Type.Magnitude_16_Concept.Magnitude_1419;
        this.Count_1412 = AlignedBox3D_47_Type.Vector_13_Concept.Count_1412;
        this.At_1415 = AlignedBox3D_47_Type.Vector_13_Concept.At_1415;
        this.Min_1480 = AlignedBox3D_47_Type.Interval_23_Concept.Min_1480;
        this.Max_1482 = AlignedBox3D_47_Type.Interval_23_Concept.Max_1482;
    }
    // field accessors
    static A_390 = function(self) { return self.A_390; }
    static B_394 = function(self) { return self.B_394; }
    // implemented concepts 
    static Array_24_Concept = new Array_24_Concept(AlignedBox3D_47_Type);
    static Value_22_Concept = new Value_22_Concept(AlignedBox3D_47_Type);
    static Arithmetic_19_Concept = new Arithmetic_19_Concept(AlignedBox3D_47_Type);
    static Equatable_18_Concept = new Equatable_18_Concept(AlignedBox3D_47_Type);
    static Comparable_17_Concept = new Comparable_17_Concept(AlignedBox3D_47_Type);
    static Magnitude_16_Concept = new Magnitude_16_Concept(AlignedBox3D_47_Type);
    static Numerical_15_Concept = new Numerical_15_Concept(AlignedBox3D_47_Type);
    static Vector_13_Concept = new Vector_13_Concept(AlignedBox3D_47_Type);
    static Interval_23_Concept = new Interval_23_Concept(AlignedBox3D_47_Type);
    static Implements = [Array_24_Concept,Value_22_Concept,Arithmetic_19_Concept,Equatable_18_Concept,Comparable_17_Concept,Magnitude_16_Concept,Numerical_15_Concept,Vector_13_Concept,Interval_23_Concept];
}
class Complex_48_Type
{
    constructor(Real_398, Imaginary_402)
    {
        // field initialization 
        this.Real_398 = Real_398;
        this.Imaginary_402 = Imaginary_402;
        this.Count_1485 = Complex_48_Type.Array_24_Concept.Count_1485;
        this.At_1488 = Complex_48_Type.Array_24_Concept.At_1488;
        this.Type_1466 = Complex_48_Type.Value_22_Concept.Type_1466;
        this.FieldTypes_1467 = Complex_48_Type.Value_22_Concept.FieldTypes_1467;
        this.FieldNames_1468 = Complex_48_Type.Value_22_Concept.FieldNames_1468;
        this.FieldValues_1470 = Complex_48_Type.Value_22_Concept.FieldValues_1470;
        this.Zero_1471 = Complex_48_Type.Value_22_Concept.Zero_1471;
        this.One_1472 = Complex_48_Type.Value_22_Concept.One_1472;
        this.Default_1473 = Complex_48_Type.Value_22_Concept.Default_1473;
        this.MinValue_1474 = Complex_48_Type.Value_22_Concept.MinValue_1474;
        this.MaxValue_1475 = Complex_48_Type.Value_22_Concept.MaxValue_1475;
        this.ToString_1477 = Complex_48_Type.Value_22_Concept.ToString_1477;
        this.Add_1428 = Complex_48_Type.Arithmetic_19_Concept.Add_1428;
        this.Negative_1430 = Complex_48_Type.Arithmetic_19_Concept.Negative_1430;
        this.Reciprocal_1432 = Complex_48_Type.Arithmetic_19_Concept.Reciprocal_1432;
        this.Multiply_1435 = Complex_48_Type.Arithmetic_19_Concept.Multiply_1435;
        this.Divide_1438 = Complex_48_Type.Arithmetic_19_Concept.Divide_1438;
        this.Modulo_1441 = Complex_48_Type.Arithmetic_19_Concept.Modulo_1441;
        this.Equals_1425 = Complex_48_Type.Equatable_18_Concept.Equals_1425;
        this.Compare_1422 = Complex_48_Type.Comparable_17_Concept.Compare_1422;
        this.Magnitude_1419 = Complex_48_Type.Magnitude_16_Concept.Magnitude_1419;
        this.Count_1412 = Complex_48_Type.Vector_13_Concept.Count_1412;
        this.At_1415 = Complex_48_Type.Vector_13_Concept.At_1415;
    }
    // field accessors
    static Real_398 = function(self) { return self.Real_398; }
    static Imaginary_402 = function(self) { return self.Imaginary_402; }
    // implemented concepts 
    static Array_24_Concept = new Array_24_Concept(Complex_48_Type);
    static Value_22_Concept = new Value_22_Concept(Complex_48_Type);
    static Arithmetic_19_Concept = new Arithmetic_19_Concept(Complex_48_Type);
    static Equatable_18_Concept = new Equatable_18_Concept(Complex_48_Type);
    static Comparable_17_Concept = new Comparable_17_Concept(Complex_48_Type);
    static Magnitude_16_Concept = new Magnitude_16_Concept(Complex_48_Type);
    static Numerical_15_Concept = new Numerical_15_Concept(Complex_48_Type);
    static Vector_13_Concept = new Vector_13_Concept(Complex_48_Type);
    static Implements = [Array_24_Concept,Value_22_Concept,Arithmetic_19_Concept,Equatable_18_Concept,Comparable_17_Concept,Magnitude_16_Concept,Numerical_15_Concept,Vector_13_Concept];
}
class Ray3D_49_Type
{
    constructor(Direction_406, Position_410)
    {
        // field initialization 
        this.Direction_406 = Direction_406;
        this.Position_410 = Position_410;
        this.Type_1466 = Ray3D_49_Type.Value_22_Concept.Type_1466;
        this.FieldTypes_1467 = Ray3D_49_Type.Value_22_Concept.FieldTypes_1467;
        this.FieldNames_1468 = Ray3D_49_Type.Value_22_Concept.FieldNames_1468;
        this.FieldValues_1470 = Ray3D_49_Type.Value_22_Concept.FieldValues_1470;
        this.Zero_1471 = Ray3D_49_Type.Value_22_Concept.Zero_1471;
        this.One_1472 = Ray3D_49_Type.Value_22_Concept.One_1472;
        this.Default_1473 = Ray3D_49_Type.Value_22_Concept.Default_1473;
        this.MinValue_1474 = Ray3D_49_Type.Value_22_Concept.MinValue_1474;
        this.MaxValue_1475 = Ray3D_49_Type.Value_22_Concept.MaxValue_1475;
        this.ToString_1477 = Ray3D_49_Type.Value_22_Concept.ToString_1477;
    }
    // field accessors
    static Direction_406 = function(self) { return self.Direction_406; }
    static Position_410 = function(self) { return self.Position_410; }
    // implemented concepts 
    static Value_22_Concept = new Value_22_Concept(Ray3D_49_Type);
    static Implements = [Value_22_Concept];
}
class Ray2D_50_Type
{
    constructor(Direction_414, Position_418)
    {
        // field initialization 
        this.Direction_414 = Direction_414;
        this.Position_418 = Position_418;
        this.Type_1466 = Ray2D_50_Type.Value_22_Concept.Type_1466;
        this.FieldTypes_1467 = Ray2D_50_Type.Value_22_Concept.FieldTypes_1467;
        this.FieldNames_1468 = Ray2D_50_Type.Value_22_Concept.FieldNames_1468;
        this.FieldValues_1470 = Ray2D_50_Type.Value_22_Concept.FieldValues_1470;
        this.Zero_1471 = Ray2D_50_Type.Value_22_Concept.Zero_1471;
        this.One_1472 = Ray2D_50_Type.Value_22_Concept.One_1472;
        this.Default_1473 = Ray2D_50_Type.Value_22_Concept.Default_1473;
        this.MinValue_1474 = Ray2D_50_Type.Value_22_Concept.MinValue_1474;
        this.MaxValue_1475 = Ray2D_50_Type.Value_22_Concept.MaxValue_1475;
        this.ToString_1477 = Ray2D_50_Type.Value_22_Concept.ToString_1477;
    }
    // field accessors
    static Direction_414 = function(self) { return self.Direction_414; }
    static Position_418 = function(self) { return self.Position_418; }
    // implemented concepts 
    static Value_22_Concept = new Value_22_Concept(Ray2D_50_Type);
    static Implements = [Value_22_Concept];
}
class Sphere_51_Type
{
    constructor(Center_422, Radius_426)
    {
        // field initialization 
        this.Center_422 = Center_422;
        this.Radius_426 = Radius_426;
        this.Type_1466 = Sphere_51_Type.Value_22_Concept.Type_1466;
        this.FieldTypes_1467 = Sphere_51_Type.Value_22_Concept.FieldTypes_1467;
        this.FieldNames_1468 = Sphere_51_Type.Value_22_Concept.FieldNames_1468;
        this.FieldValues_1470 = Sphere_51_Type.Value_22_Concept.FieldValues_1470;
        this.Zero_1471 = Sphere_51_Type.Value_22_Concept.Zero_1471;
        this.One_1472 = Sphere_51_Type.Value_22_Concept.One_1472;
        this.Default_1473 = Sphere_51_Type.Value_22_Concept.Default_1473;
        this.MinValue_1474 = Sphere_51_Type.Value_22_Concept.MinValue_1474;
        this.MaxValue_1475 = Sphere_51_Type.Value_22_Concept.MaxValue_1475;
        this.ToString_1477 = Sphere_51_Type.Value_22_Concept.ToString_1477;
    }
    // field accessors
    static Center_422 = function(self) { return self.Center_422; }
    static Radius_426 = function(self) { return self.Radius_426; }
    // implemented concepts 
    static Value_22_Concept = new Value_22_Concept(Sphere_51_Type);
    static Implements = [Value_22_Concept];
}
class Plane_52_Type
{
    constructor(Normal_430, D_434)
    {
        // field initialization 
        this.Normal_430 = Normal_430;
        this.D_434 = D_434;
        this.Type_1466 = Plane_52_Type.Value_22_Concept.Type_1466;
        this.FieldTypes_1467 = Plane_52_Type.Value_22_Concept.FieldTypes_1467;
        this.FieldNames_1468 = Plane_52_Type.Value_22_Concept.FieldNames_1468;
        this.FieldValues_1470 = Plane_52_Type.Value_22_Concept.FieldValues_1470;
        this.Zero_1471 = Plane_52_Type.Value_22_Concept.Zero_1471;
        this.One_1472 = Plane_52_Type.Value_22_Concept.One_1472;
        this.Default_1473 = Plane_52_Type.Value_22_Concept.Default_1473;
        this.MinValue_1474 = Plane_52_Type.Value_22_Concept.MinValue_1474;
        this.MaxValue_1475 = Plane_52_Type.Value_22_Concept.MaxValue_1475;
        this.ToString_1477 = Plane_52_Type.Value_22_Concept.ToString_1477;
    }
    // field accessors
    static Normal_430 = function(self) { return self.Normal_430; }
    static D_434 = function(self) { return self.D_434; }
    // implemented concepts 
    static Value_22_Concept = new Value_22_Concept(Plane_52_Type);
    static Implements = [Value_22_Concept];
}
class Triangle3D_53_Type
{
    constructor(A_438, B_442, C_446)
    {
        // field initialization 
        this.A_438 = A_438;
        this.B_442 = B_442;
        this.C_446 = C_446;
        this.Type_1466 = Triangle3D_53_Type.Value_22_Concept.Type_1466;
        this.FieldTypes_1467 = Triangle3D_53_Type.Value_22_Concept.FieldTypes_1467;
        this.FieldNames_1468 = Triangle3D_53_Type.Value_22_Concept.FieldNames_1468;
        this.FieldValues_1470 = Triangle3D_53_Type.Value_22_Concept.FieldValues_1470;
        this.Zero_1471 = Triangle3D_53_Type.Value_22_Concept.Zero_1471;
        this.One_1472 = Triangle3D_53_Type.Value_22_Concept.One_1472;
        this.Default_1473 = Triangle3D_53_Type.Value_22_Concept.Default_1473;
        this.MinValue_1474 = Triangle3D_53_Type.Value_22_Concept.MinValue_1474;
        this.MaxValue_1475 = Triangle3D_53_Type.Value_22_Concept.MaxValue_1475;
        this.ToString_1477 = Triangle3D_53_Type.Value_22_Concept.ToString_1477;
    }
    // field accessors
    static A_438 = function(self) { return self.A_438; }
    static B_442 = function(self) { return self.B_442; }
    static C_446 = function(self) { return self.C_446; }
    // implemented concepts 
    static Value_22_Concept = new Value_22_Concept(Triangle3D_53_Type);
    static Implements = [Value_22_Concept];
}
class Triangle2D_54_Type
{
    constructor(A_450, B_454, C_458)
    {
        // field initialization 
        this.A_450 = A_450;
        this.B_454 = B_454;
        this.C_458 = C_458;
        this.Type_1466 = Triangle2D_54_Type.Value_22_Concept.Type_1466;
        this.FieldTypes_1467 = Triangle2D_54_Type.Value_22_Concept.FieldTypes_1467;
        this.FieldNames_1468 = Triangle2D_54_Type.Value_22_Concept.FieldNames_1468;
        this.FieldValues_1470 = Triangle2D_54_Type.Value_22_Concept.FieldValues_1470;
        this.Zero_1471 = Triangle2D_54_Type.Value_22_Concept.Zero_1471;
        this.One_1472 = Triangle2D_54_Type.Value_22_Concept.One_1472;
        this.Default_1473 = Triangle2D_54_Type.Value_22_Concept.Default_1473;
        this.MinValue_1474 = Triangle2D_54_Type.Value_22_Concept.MinValue_1474;
        this.MaxValue_1475 = Triangle2D_54_Type.Value_22_Concept.MaxValue_1475;
        this.ToString_1477 = Triangle2D_54_Type.Value_22_Concept.ToString_1477;
    }
    // field accessors
    static A_450 = function(self) { return self.A_450; }
    static B_454 = function(self) { return self.B_454; }
    static C_458 = function(self) { return self.C_458; }
    // implemented concepts 
    static Value_22_Concept = new Value_22_Concept(Triangle2D_54_Type);
    static Implements = [Value_22_Concept];
}
class Quad3D_55_Type
{
    constructor(A_462, B_466, C_470, D_474)
    {
        // field initialization 
        this.A_462 = A_462;
        this.B_466 = B_466;
        this.C_470 = C_470;
        this.D_474 = D_474;
        this.Type_1466 = Quad3D_55_Type.Value_22_Concept.Type_1466;
        this.FieldTypes_1467 = Quad3D_55_Type.Value_22_Concept.FieldTypes_1467;
        this.FieldNames_1468 = Quad3D_55_Type.Value_22_Concept.FieldNames_1468;
        this.FieldValues_1470 = Quad3D_55_Type.Value_22_Concept.FieldValues_1470;
        this.Zero_1471 = Quad3D_55_Type.Value_22_Concept.Zero_1471;
        this.One_1472 = Quad3D_55_Type.Value_22_Concept.One_1472;
        this.Default_1473 = Quad3D_55_Type.Value_22_Concept.Default_1473;
        this.MinValue_1474 = Quad3D_55_Type.Value_22_Concept.MinValue_1474;
        this.MaxValue_1475 = Quad3D_55_Type.Value_22_Concept.MaxValue_1475;
        this.ToString_1477 = Quad3D_55_Type.Value_22_Concept.ToString_1477;
    }
    // field accessors
    static A_462 = function(self) { return self.A_462; }
    static B_466 = function(self) { return self.B_466; }
    static C_470 = function(self) { return self.C_470; }
    static D_474 = function(self) { return self.D_474; }
    // implemented concepts 
    static Value_22_Concept = new Value_22_Concept(Quad3D_55_Type);
    static Implements = [Value_22_Concept];
}
class Quad2D_56_Type
{
    constructor(A_478, B_482, C_486, D_490)
    {
        // field initialization 
        this.A_478 = A_478;
        this.B_482 = B_482;
        this.C_486 = C_486;
        this.D_490 = D_490;
        this.Type_1466 = Quad2D_56_Type.Value_22_Concept.Type_1466;
        this.FieldTypes_1467 = Quad2D_56_Type.Value_22_Concept.FieldTypes_1467;
        this.FieldNames_1468 = Quad2D_56_Type.Value_22_Concept.FieldNames_1468;
        this.FieldValues_1470 = Quad2D_56_Type.Value_22_Concept.FieldValues_1470;
        this.Zero_1471 = Quad2D_56_Type.Value_22_Concept.Zero_1471;
        this.One_1472 = Quad2D_56_Type.Value_22_Concept.One_1472;
        this.Default_1473 = Quad2D_56_Type.Value_22_Concept.Default_1473;
        this.MinValue_1474 = Quad2D_56_Type.Value_22_Concept.MinValue_1474;
        this.MaxValue_1475 = Quad2D_56_Type.Value_22_Concept.MaxValue_1475;
        this.ToString_1477 = Quad2D_56_Type.Value_22_Concept.ToString_1477;
    }
    // field accessors
    static A_478 = function(self) { return self.A_478; }
    static B_482 = function(self) { return self.B_482; }
    static C_486 = function(self) { return self.C_486; }
    static D_490 = function(self) { return self.D_490; }
    // implemented concepts 
    static Value_22_Concept = new Value_22_Concept(Quad2D_56_Type);
    static Implements = [Value_22_Concept];
}
class Point3D_57_Type
{
    constructor(Value_494)
    {
        // field initialization 
        this.Value_494 = Value_494;
        this.Type_1466 = Point3D_57_Type.Value_22_Concept.Type_1466;
        this.FieldTypes_1467 = Point3D_57_Type.Value_22_Concept.FieldTypes_1467;
        this.FieldNames_1468 = Point3D_57_Type.Value_22_Concept.FieldNames_1468;
        this.FieldValues_1470 = Point3D_57_Type.Value_22_Concept.FieldValues_1470;
        this.Zero_1471 = Point3D_57_Type.Value_22_Concept.Zero_1471;
        this.One_1472 = Point3D_57_Type.Value_22_Concept.One_1472;
        this.Default_1473 = Point3D_57_Type.Value_22_Concept.Default_1473;
        this.MinValue_1474 = Point3D_57_Type.Value_22_Concept.MinValue_1474;
        this.MaxValue_1475 = Point3D_57_Type.Value_22_Concept.MaxValue_1475;
        this.ToString_1477 = Point3D_57_Type.Value_22_Concept.ToString_1477;
    }
    // field accessors
    static Value_494 = function(self) { return self.Value_494; }
    // implemented concepts 
    static Value_22_Concept = new Value_22_Concept(Point3D_57_Type);
    static Implements = [Value_22_Concept];
}
class Point2D_58_Type
{
    constructor(Value_498)
    {
        // field initialization 
        this.Value_498 = Value_498;
        this.Type_1466 = Point2D_58_Type.Value_22_Concept.Type_1466;
        this.FieldTypes_1467 = Point2D_58_Type.Value_22_Concept.FieldTypes_1467;
        this.FieldNames_1468 = Point2D_58_Type.Value_22_Concept.FieldNames_1468;
        this.FieldValues_1470 = Point2D_58_Type.Value_22_Concept.FieldValues_1470;
        this.Zero_1471 = Point2D_58_Type.Value_22_Concept.Zero_1471;
        this.One_1472 = Point2D_58_Type.Value_22_Concept.One_1472;
        this.Default_1473 = Point2D_58_Type.Value_22_Concept.Default_1473;
        this.MinValue_1474 = Point2D_58_Type.Value_22_Concept.MinValue_1474;
        this.MaxValue_1475 = Point2D_58_Type.Value_22_Concept.MaxValue_1475;
        this.ToString_1477 = Point2D_58_Type.Value_22_Concept.ToString_1477;
    }
    // field accessors
    static Value_498 = function(self) { return self.Value_498; }
    // implemented concepts 
    static Value_22_Concept = new Value_22_Concept(Point2D_58_Type);
    static Implements = [Value_22_Concept];
}
class Line3D_59_Type
{
    constructor(A_502, B_506)
    {
        // field initialization 
        this.A_502 = A_502;
        this.B_506 = B_506;
        this.Count_1485 = Line3D_59_Type.Array_24_Concept.Count_1485;
        this.At_1488 = Line3D_59_Type.Array_24_Concept.At_1488;
        this.Type_1466 = Line3D_59_Type.Value_22_Concept.Type_1466;
        this.FieldTypes_1467 = Line3D_59_Type.Value_22_Concept.FieldTypes_1467;
        this.FieldNames_1468 = Line3D_59_Type.Value_22_Concept.FieldNames_1468;
        this.FieldValues_1470 = Line3D_59_Type.Value_22_Concept.FieldValues_1470;
        this.Zero_1471 = Line3D_59_Type.Value_22_Concept.Zero_1471;
        this.One_1472 = Line3D_59_Type.Value_22_Concept.One_1472;
        this.Default_1473 = Line3D_59_Type.Value_22_Concept.Default_1473;
        this.MinValue_1474 = Line3D_59_Type.Value_22_Concept.MinValue_1474;
        this.MaxValue_1475 = Line3D_59_Type.Value_22_Concept.MaxValue_1475;
        this.ToString_1477 = Line3D_59_Type.Value_22_Concept.ToString_1477;
        this.Add_1428 = Line3D_59_Type.Arithmetic_19_Concept.Add_1428;
        this.Negative_1430 = Line3D_59_Type.Arithmetic_19_Concept.Negative_1430;
        this.Reciprocal_1432 = Line3D_59_Type.Arithmetic_19_Concept.Reciprocal_1432;
        this.Multiply_1435 = Line3D_59_Type.Arithmetic_19_Concept.Multiply_1435;
        this.Divide_1438 = Line3D_59_Type.Arithmetic_19_Concept.Divide_1438;
        this.Modulo_1441 = Line3D_59_Type.Arithmetic_19_Concept.Modulo_1441;
        this.Equals_1425 = Line3D_59_Type.Equatable_18_Concept.Equals_1425;
        this.Compare_1422 = Line3D_59_Type.Comparable_17_Concept.Compare_1422;
        this.Magnitude_1419 = Line3D_59_Type.Magnitude_16_Concept.Magnitude_1419;
        this.Count_1412 = Line3D_59_Type.Vector_13_Concept.Count_1412;
        this.At_1415 = Line3D_59_Type.Vector_13_Concept.At_1415;
        this.Min_1480 = Line3D_59_Type.Interval_23_Concept.Min_1480;
        this.Max_1482 = Line3D_59_Type.Interval_23_Concept.Max_1482;
    }
    // field accessors
    static A_502 = function(self) { return self.A_502; }
    static B_506 = function(self) { return self.B_506; }
    // implemented concepts 
    static Array_24_Concept = new Array_24_Concept(Line3D_59_Type);
    static Value_22_Concept = new Value_22_Concept(Line3D_59_Type);
    static Arithmetic_19_Concept = new Arithmetic_19_Concept(Line3D_59_Type);
    static Equatable_18_Concept = new Equatable_18_Concept(Line3D_59_Type);
    static Comparable_17_Concept = new Comparable_17_Concept(Line3D_59_Type);
    static Magnitude_16_Concept = new Magnitude_16_Concept(Line3D_59_Type);
    static Numerical_15_Concept = new Numerical_15_Concept(Line3D_59_Type);
    static Vector_13_Concept = new Vector_13_Concept(Line3D_59_Type);
    static Interval_23_Concept = new Interval_23_Concept(Line3D_59_Type);
    static Implements = [Array_24_Concept,Value_22_Concept,Arithmetic_19_Concept,Equatable_18_Concept,Comparable_17_Concept,Magnitude_16_Concept,Numerical_15_Concept,Vector_13_Concept,Interval_23_Concept];
}
class Line2D_60_Type
{
    constructor(A_510, B_514)
    {
        // field initialization 
        this.A_510 = A_510;
        this.B_514 = B_514;
        this.Count_1485 = Line2D_60_Type.Array_24_Concept.Count_1485;
        this.At_1488 = Line2D_60_Type.Array_24_Concept.At_1488;
        this.Type_1466 = Line2D_60_Type.Value_22_Concept.Type_1466;
        this.FieldTypes_1467 = Line2D_60_Type.Value_22_Concept.FieldTypes_1467;
        this.FieldNames_1468 = Line2D_60_Type.Value_22_Concept.FieldNames_1468;
        this.FieldValues_1470 = Line2D_60_Type.Value_22_Concept.FieldValues_1470;
        this.Zero_1471 = Line2D_60_Type.Value_22_Concept.Zero_1471;
        this.One_1472 = Line2D_60_Type.Value_22_Concept.One_1472;
        this.Default_1473 = Line2D_60_Type.Value_22_Concept.Default_1473;
        this.MinValue_1474 = Line2D_60_Type.Value_22_Concept.MinValue_1474;
        this.MaxValue_1475 = Line2D_60_Type.Value_22_Concept.MaxValue_1475;
        this.ToString_1477 = Line2D_60_Type.Value_22_Concept.ToString_1477;
        this.Add_1428 = Line2D_60_Type.Arithmetic_19_Concept.Add_1428;
        this.Negative_1430 = Line2D_60_Type.Arithmetic_19_Concept.Negative_1430;
        this.Reciprocal_1432 = Line2D_60_Type.Arithmetic_19_Concept.Reciprocal_1432;
        this.Multiply_1435 = Line2D_60_Type.Arithmetic_19_Concept.Multiply_1435;
        this.Divide_1438 = Line2D_60_Type.Arithmetic_19_Concept.Divide_1438;
        this.Modulo_1441 = Line2D_60_Type.Arithmetic_19_Concept.Modulo_1441;
        this.Equals_1425 = Line2D_60_Type.Equatable_18_Concept.Equals_1425;
        this.Compare_1422 = Line2D_60_Type.Comparable_17_Concept.Compare_1422;
        this.Magnitude_1419 = Line2D_60_Type.Magnitude_16_Concept.Magnitude_1419;
        this.Count_1412 = Line2D_60_Type.Vector_13_Concept.Count_1412;
        this.At_1415 = Line2D_60_Type.Vector_13_Concept.At_1415;
        this.Min_1480 = Line2D_60_Type.Interval_23_Concept.Min_1480;
        this.Max_1482 = Line2D_60_Type.Interval_23_Concept.Max_1482;
    }
    // field accessors
    static A_510 = function(self) { return self.A_510; }
    static B_514 = function(self) { return self.B_514; }
    // implemented concepts 
    static Array_24_Concept = new Array_24_Concept(Line2D_60_Type);
    static Value_22_Concept = new Value_22_Concept(Line2D_60_Type);
    static Arithmetic_19_Concept = new Arithmetic_19_Concept(Line2D_60_Type);
    static Equatable_18_Concept = new Equatable_18_Concept(Line2D_60_Type);
    static Comparable_17_Concept = new Comparable_17_Concept(Line2D_60_Type);
    static Magnitude_16_Concept = new Magnitude_16_Concept(Line2D_60_Type);
    static Numerical_15_Concept = new Numerical_15_Concept(Line2D_60_Type);
    static Vector_13_Concept = new Vector_13_Concept(Line2D_60_Type);
    static Interval_23_Concept = new Interval_23_Concept(Line2D_60_Type);
    static Implements = [Array_24_Concept,Value_22_Concept,Arithmetic_19_Concept,Equatable_18_Concept,Comparable_17_Concept,Magnitude_16_Concept,Numerical_15_Concept,Vector_13_Concept,Interval_23_Concept];
}
class Color_61_Type
{
    constructor(R_518, G_522, B_526, A_530)
    {
        // field initialization 
        this.R_518 = R_518;
        this.G_522 = G_522;
        this.B_526 = B_526;
        this.A_530 = A_530;
        this.Type_1466 = Color_61_Type.Value_22_Concept.Type_1466;
        this.FieldTypes_1467 = Color_61_Type.Value_22_Concept.FieldTypes_1467;
        this.FieldNames_1468 = Color_61_Type.Value_22_Concept.FieldNames_1468;
        this.FieldValues_1470 = Color_61_Type.Value_22_Concept.FieldValues_1470;
        this.Zero_1471 = Color_61_Type.Value_22_Concept.Zero_1471;
        this.One_1472 = Color_61_Type.Value_22_Concept.One_1472;
        this.Default_1473 = Color_61_Type.Value_22_Concept.Default_1473;
        this.MinValue_1474 = Color_61_Type.Value_22_Concept.MinValue_1474;
        this.MaxValue_1475 = Color_61_Type.Value_22_Concept.MaxValue_1475;
        this.ToString_1477 = Color_61_Type.Value_22_Concept.ToString_1477;
    }
    // field accessors
    static R_518 = function(self) { return self.R_518; }
    static G_522 = function(self) { return self.G_522; }
    static B_526 = function(self) { return self.B_526; }
    static A_530 = function(self) { return self.A_530; }
    // implemented concepts 
    static Value_22_Concept = new Value_22_Concept(Color_61_Type);
    static Implements = [Value_22_Concept];
}
class ColorLUV_62_Type
{
    constructor(Lightness_534, U_538, V_542)
    {
        // field initialization 
        this.Lightness_534 = Lightness_534;
        this.U_538 = U_538;
        this.V_542 = V_542;
        this.Type_1466 = ColorLUV_62_Type.Value_22_Concept.Type_1466;
        this.FieldTypes_1467 = ColorLUV_62_Type.Value_22_Concept.FieldTypes_1467;
        this.FieldNames_1468 = ColorLUV_62_Type.Value_22_Concept.FieldNames_1468;
        this.FieldValues_1470 = ColorLUV_62_Type.Value_22_Concept.FieldValues_1470;
        this.Zero_1471 = ColorLUV_62_Type.Value_22_Concept.Zero_1471;
        this.One_1472 = ColorLUV_62_Type.Value_22_Concept.One_1472;
        this.Default_1473 = ColorLUV_62_Type.Value_22_Concept.Default_1473;
        this.MinValue_1474 = ColorLUV_62_Type.Value_22_Concept.MinValue_1474;
        this.MaxValue_1475 = ColorLUV_62_Type.Value_22_Concept.MaxValue_1475;
        this.ToString_1477 = ColorLUV_62_Type.Value_22_Concept.ToString_1477;
    }
    // field accessors
    static Lightness_534 = function(self) { return self.Lightness_534; }
    static U_538 = function(self) { return self.U_538; }
    static V_542 = function(self) { return self.V_542; }
    // implemented concepts 
    static Value_22_Concept = new Value_22_Concept(ColorLUV_62_Type);
    static Implements = [Value_22_Concept];
}
class ColorLAB_63_Type
{
    constructor(Lightness_546, A_550, B_554)
    {
        // field initialization 
        this.Lightness_546 = Lightness_546;
        this.A_550 = A_550;
        this.B_554 = B_554;
        this.Type_1466 = ColorLAB_63_Type.Value_22_Concept.Type_1466;
        this.FieldTypes_1467 = ColorLAB_63_Type.Value_22_Concept.FieldTypes_1467;
        this.FieldNames_1468 = ColorLAB_63_Type.Value_22_Concept.FieldNames_1468;
        this.FieldValues_1470 = ColorLAB_63_Type.Value_22_Concept.FieldValues_1470;
        this.Zero_1471 = ColorLAB_63_Type.Value_22_Concept.Zero_1471;
        this.One_1472 = ColorLAB_63_Type.Value_22_Concept.One_1472;
        this.Default_1473 = ColorLAB_63_Type.Value_22_Concept.Default_1473;
        this.MinValue_1474 = ColorLAB_63_Type.Value_22_Concept.MinValue_1474;
        this.MaxValue_1475 = ColorLAB_63_Type.Value_22_Concept.MaxValue_1475;
        this.ToString_1477 = ColorLAB_63_Type.Value_22_Concept.ToString_1477;
    }
    // field accessors
    static Lightness_546 = function(self) { return self.Lightness_546; }
    static A_550 = function(self) { return self.A_550; }
    static B_554 = function(self) { return self.B_554; }
    // implemented concepts 
    static Value_22_Concept = new Value_22_Concept(ColorLAB_63_Type);
    static Implements = [Value_22_Concept];
}
class ColorLCh_64_Type
{
    constructor(Lightness_558, ChromaHue_562)
    {
        // field initialization 
        this.Lightness_558 = Lightness_558;
        this.ChromaHue_562 = ChromaHue_562;
        this.Type_1466 = ColorLCh_64_Type.Value_22_Concept.Type_1466;
        this.FieldTypes_1467 = ColorLCh_64_Type.Value_22_Concept.FieldTypes_1467;
        this.FieldNames_1468 = ColorLCh_64_Type.Value_22_Concept.FieldNames_1468;
        this.FieldValues_1470 = ColorLCh_64_Type.Value_22_Concept.FieldValues_1470;
        this.Zero_1471 = ColorLCh_64_Type.Value_22_Concept.Zero_1471;
        this.One_1472 = ColorLCh_64_Type.Value_22_Concept.One_1472;
        this.Default_1473 = ColorLCh_64_Type.Value_22_Concept.Default_1473;
        this.MinValue_1474 = ColorLCh_64_Type.Value_22_Concept.MinValue_1474;
        this.MaxValue_1475 = ColorLCh_64_Type.Value_22_Concept.MaxValue_1475;
        this.ToString_1477 = ColorLCh_64_Type.Value_22_Concept.ToString_1477;
    }
    // field accessors
    static Lightness_558 = function(self) { return self.Lightness_558; }
    static ChromaHue_562 = function(self) { return self.ChromaHue_562; }
    // implemented concepts 
    static Value_22_Concept = new Value_22_Concept(ColorLCh_64_Type);
    static Implements = [Value_22_Concept];
}
class ColorHSV_65_Type
{
    constructor(Hue_566, S_570, V_574)
    {
        // field initialization 
        this.Hue_566 = Hue_566;
        this.S_570 = S_570;
        this.V_574 = V_574;
        this.Type_1466 = ColorHSV_65_Type.Value_22_Concept.Type_1466;
        this.FieldTypes_1467 = ColorHSV_65_Type.Value_22_Concept.FieldTypes_1467;
        this.FieldNames_1468 = ColorHSV_65_Type.Value_22_Concept.FieldNames_1468;
        this.FieldValues_1470 = ColorHSV_65_Type.Value_22_Concept.FieldValues_1470;
        this.Zero_1471 = ColorHSV_65_Type.Value_22_Concept.Zero_1471;
        this.One_1472 = ColorHSV_65_Type.Value_22_Concept.One_1472;
        this.Default_1473 = ColorHSV_65_Type.Value_22_Concept.Default_1473;
        this.MinValue_1474 = ColorHSV_65_Type.Value_22_Concept.MinValue_1474;
        this.MaxValue_1475 = ColorHSV_65_Type.Value_22_Concept.MaxValue_1475;
        this.ToString_1477 = ColorHSV_65_Type.Value_22_Concept.ToString_1477;
    }
    // field accessors
    static Hue_566 = function(self) { return self.Hue_566; }
    static S_570 = function(self) { return self.S_570; }
    static V_574 = function(self) { return self.V_574; }
    // implemented concepts 
    static Value_22_Concept = new Value_22_Concept(ColorHSV_65_Type);
    static Implements = [Value_22_Concept];
}
class ColorHSL_66_Type
{
    constructor(Hue_578, Saturation_582, Luminance_586)
    {
        // field initialization 
        this.Hue_578 = Hue_578;
        this.Saturation_582 = Saturation_582;
        this.Luminance_586 = Luminance_586;
        this.Type_1466 = ColorHSL_66_Type.Value_22_Concept.Type_1466;
        this.FieldTypes_1467 = ColorHSL_66_Type.Value_22_Concept.FieldTypes_1467;
        this.FieldNames_1468 = ColorHSL_66_Type.Value_22_Concept.FieldNames_1468;
        this.FieldValues_1470 = ColorHSL_66_Type.Value_22_Concept.FieldValues_1470;
        this.Zero_1471 = ColorHSL_66_Type.Value_22_Concept.Zero_1471;
        this.One_1472 = ColorHSL_66_Type.Value_22_Concept.One_1472;
        this.Default_1473 = ColorHSL_66_Type.Value_22_Concept.Default_1473;
        this.MinValue_1474 = ColorHSL_66_Type.Value_22_Concept.MinValue_1474;
        this.MaxValue_1475 = ColorHSL_66_Type.Value_22_Concept.MaxValue_1475;
        this.ToString_1477 = ColorHSL_66_Type.Value_22_Concept.ToString_1477;
    }
    // field accessors
    static Hue_578 = function(self) { return self.Hue_578; }
    static Saturation_582 = function(self) { return self.Saturation_582; }
    static Luminance_586 = function(self) { return self.Luminance_586; }
    // implemented concepts 
    static Value_22_Concept = new Value_22_Concept(ColorHSL_66_Type);
    static Implements = [Value_22_Concept];
}
class ColorYCbCr_67_Type
{
    constructor(Y_590, Cb_594, Cr_598)
    {
        // field initialization 
        this.Y_590 = Y_590;
        this.Cb_594 = Cb_594;
        this.Cr_598 = Cr_598;
        this.Type_1466 = ColorYCbCr_67_Type.Value_22_Concept.Type_1466;
        this.FieldTypes_1467 = ColorYCbCr_67_Type.Value_22_Concept.FieldTypes_1467;
        this.FieldNames_1468 = ColorYCbCr_67_Type.Value_22_Concept.FieldNames_1468;
        this.FieldValues_1470 = ColorYCbCr_67_Type.Value_22_Concept.FieldValues_1470;
        this.Zero_1471 = ColorYCbCr_67_Type.Value_22_Concept.Zero_1471;
        this.One_1472 = ColorYCbCr_67_Type.Value_22_Concept.One_1472;
        this.Default_1473 = ColorYCbCr_67_Type.Value_22_Concept.Default_1473;
        this.MinValue_1474 = ColorYCbCr_67_Type.Value_22_Concept.MinValue_1474;
        this.MaxValue_1475 = ColorYCbCr_67_Type.Value_22_Concept.MaxValue_1475;
        this.ToString_1477 = ColorYCbCr_67_Type.Value_22_Concept.ToString_1477;
    }
    // field accessors
    static Y_590 = function(self) { return self.Y_590; }
    static Cb_594 = function(self) { return self.Cb_594; }
    static Cr_598 = function(self) { return self.Cr_598; }
    // implemented concepts 
    static Value_22_Concept = new Value_22_Concept(ColorYCbCr_67_Type);
    static Implements = [Value_22_Concept];
}
class SphericalCoordinate_68_Type
{
    constructor(Radius_602, Azimuth_606, Polar_610)
    {
        // field initialization 
        this.Radius_602 = Radius_602;
        this.Azimuth_606 = Azimuth_606;
        this.Polar_610 = Polar_610;
        this.Type_1466 = SphericalCoordinate_68_Type.Value_22_Concept.Type_1466;
        this.FieldTypes_1467 = SphericalCoordinate_68_Type.Value_22_Concept.FieldTypes_1467;
        this.FieldNames_1468 = SphericalCoordinate_68_Type.Value_22_Concept.FieldNames_1468;
        this.FieldValues_1470 = SphericalCoordinate_68_Type.Value_22_Concept.FieldValues_1470;
        this.Zero_1471 = SphericalCoordinate_68_Type.Value_22_Concept.Zero_1471;
        this.One_1472 = SphericalCoordinate_68_Type.Value_22_Concept.One_1472;
        this.Default_1473 = SphericalCoordinate_68_Type.Value_22_Concept.Default_1473;
        this.MinValue_1474 = SphericalCoordinate_68_Type.Value_22_Concept.MinValue_1474;
        this.MaxValue_1475 = SphericalCoordinate_68_Type.Value_22_Concept.MaxValue_1475;
        this.ToString_1477 = SphericalCoordinate_68_Type.Value_22_Concept.ToString_1477;
    }
    // field accessors
    static Radius_602 = function(self) { return self.Radius_602; }
    static Azimuth_606 = function(self) { return self.Azimuth_606; }
    static Polar_610 = function(self) { return self.Polar_610; }
    // implemented concepts 
    static Value_22_Concept = new Value_22_Concept(SphericalCoordinate_68_Type);
    static Implements = [Value_22_Concept];
}
class PolarCoordinate_69_Type
{
    constructor(Radius_614, Angle_618)
    {
        // field initialization 
        this.Radius_614 = Radius_614;
        this.Angle_618 = Angle_618;
        this.Type_1466 = PolarCoordinate_69_Type.Value_22_Concept.Type_1466;
        this.FieldTypes_1467 = PolarCoordinate_69_Type.Value_22_Concept.FieldTypes_1467;
        this.FieldNames_1468 = PolarCoordinate_69_Type.Value_22_Concept.FieldNames_1468;
        this.FieldValues_1470 = PolarCoordinate_69_Type.Value_22_Concept.FieldValues_1470;
        this.Zero_1471 = PolarCoordinate_69_Type.Value_22_Concept.Zero_1471;
        this.One_1472 = PolarCoordinate_69_Type.Value_22_Concept.One_1472;
        this.Default_1473 = PolarCoordinate_69_Type.Value_22_Concept.Default_1473;
        this.MinValue_1474 = PolarCoordinate_69_Type.Value_22_Concept.MinValue_1474;
        this.MaxValue_1475 = PolarCoordinate_69_Type.Value_22_Concept.MaxValue_1475;
        this.ToString_1477 = PolarCoordinate_69_Type.Value_22_Concept.ToString_1477;
    }
    // field accessors
    static Radius_614 = function(self) { return self.Radius_614; }
    static Angle_618 = function(self) { return self.Angle_618; }
    // implemented concepts 
    static Value_22_Concept = new Value_22_Concept(PolarCoordinate_69_Type);
    static Implements = [Value_22_Concept];
}
class LogPolarCoordinate_70_Type
{
    constructor(Rho_622, Azimuth_626)
    {
        // field initialization 
        this.Rho_622 = Rho_622;
        this.Azimuth_626 = Azimuth_626;
        this.Type_1466 = LogPolarCoordinate_70_Type.Value_22_Concept.Type_1466;
        this.FieldTypes_1467 = LogPolarCoordinate_70_Type.Value_22_Concept.FieldTypes_1467;
        this.FieldNames_1468 = LogPolarCoordinate_70_Type.Value_22_Concept.FieldNames_1468;
        this.FieldValues_1470 = LogPolarCoordinate_70_Type.Value_22_Concept.FieldValues_1470;
        this.Zero_1471 = LogPolarCoordinate_70_Type.Value_22_Concept.Zero_1471;
        this.One_1472 = LogPolarCoordinate_70_Type.Value_22_Concept.One_1472;
        this.Default_1473 = LogPolarCoordinate_70_Type.Value_22_Concept.Default_1473;
        this.MinValue_1474 = LogPolarCoordinate_70_Type.Value_22_Concept.MinValue_1474;
        this.MaxValue_1475 = LogPolarCoordinate_70_Type.Value_22_Concept.MaxValue_1475;
        this.ToString_1477 = LogPolarCoordinate_70_Type.Value_22_Concept.ToString_1477;
    }
    // field accessors
    static Rho_622 = function(self) { return self.Rho_622; }
    static Azimuth_626 = function(self) { return self.Azimuth_626; }
    // implemented concepts 
    static Value_22_Concept = new Value_22_Concept(LogPolarCoordinate_70_Type);
    static Implements = [Value_22_Concept];
}
class CylindricalCoordinate_71_Type
{
    constructor(RadialDistance_630, Azimuth_634, Height_638)
    {
        // field initialization 
        this.RadialDistance_630 = RadialDistance_630;
        this.Azimuth_634 = Azimuth_634;
        this.Height_638 = Height_638;
        this.Type_1466 = CylindricalCoordinate_71_Type.Value_22_Concept.Type_1466;
        this.FieldTypes_1467 = CylindricalCoordinate_71_Type.Value_22_Concept.FieldTypes_1467;
        this.FieldNames_1468 = CylindricalCoordinate_71_Type.Value_22_Concept.FieldNames_1468;
        this.FieldValues_1470 = CylindricalCoordinate_71_Type.Value_22_Concept.FieldValues_1470;
        this.Zero_1471 = CylindricalCoordinate_71_Type.Value_22_Concept.Zero_1471;
        this.One_1472 = CylindricalCoordinate_71_Type.Value_22_Concept.One_1472;
        this.Default_1473 = CylindricalCoordinate_71_Type.Value_22_Concept.Default_1473;
        this.MinValue_1474 = CylindricalCoordinate_71_Type.Value_22_Concept.MinValue_1474;
        this.MaxValue_1475 = CylindricalCoordinate_71_Type.Value_22_Concept.MaxValue_1475;
        this.ToString_1477 = CylindricalCoordinate_71_Type.Value_22_Concept.ToString_1477;
    }
    // field accessors
    static RadialDistance_630 = function(self) { return self.RadialDistance_630; }
    static Azimuth_634 = function(self) { return self.Azimuth_634; }
    static Height_638 = function(self) { return self.Height_638; }
    // implemented concepts 
    static Value_22_Concept = new Value_22_Concept(CylindricalCoordinate_71_Type);
    static Implements = [Value_22_Concept];
}
class HorizontalCoordinate_72_Type
{
    constructor(Radius_642, Azimuth_646, Height_650)
    {
        // field initialization 
        this.Radius_642 = Radius_642;
        this.Azimuth_646 = Azimuth_646;
        this.Height_650 = Height_650;
        this.Type_1466 = HorizontalCoordinate_72_Type.Value_22_Concept.Type_1466;
        this.FieldTypes_1467 = HorizontalCoordinate_72_Type.Value_22_Concept.FieldTypes_1467;
        this.FieldNames_1468 = HorizontalCoordinate_72_Type.Value_22_Concept.FieldNames_1468;
        this.FieldValues_1470 = HorizontalCoordinate_72_Type.Value_22_Concept.FieldValues_1470;
        this.Zero_1471 = HorizontalCoordinate_72_Type.Value_22_Concept.Zero_1471;
        this.One_1472 = HorizontalCoordinate_72_Type.Value_22_Concept.One_1472;
        this.Default_1473 = HorizontalCoordinate_72_Type.Value_22_Concept.Default_1473;
        this.MinValue_1474 = HorizontalCoordinate_72_Type.Value_22_Concept.MinValue_1474;
        this.MaxValue_1475 = HorizontalCoordinate_72_Type.Value_22_Concept.MaxValue_1475;
        this.ToString_1477 = HorizontalCoordinate_72_Type.Value_22_Concept.ToString_1477;
    }
    // field accessors
    static Radius_642 = function(self) { return self.Radius_642; }
    static Azimuth_646 = function(self) { return self.Azimuth_646; }
    static Height_650 = function(self) { return self.Height_650; }
    // implemented concepts 
    static Value_22_Concept = new Value_22_Concept(HorizontalCoordinate_72_Type);
    static Implements = [Value_22_Concept];
}
class GeoCoordinate_73_Type
{
    constructor(Latitude_654, Longitude_658)
    {
        // field initialization 
        this.Latitude_654 = Latitude_654;
        this.Longitude_658 = Longitude_658;
        this.Type_1466 = GeoCoordinate_73_Type.Value_22_Concept.Type_1466;
        this.FieldTypes_1467 = GeoCoordinate_73_Type.Value_22_Concept.FieldTypes_1467;
        this.FieldNames_1468 = GeoCoordinate_73_Type.Value_22_Concept.FieldNames_1468;
        this.FieldValues_1470 = GeoCoordinate_73_Type.Value_22_Concept.FieldValues_1470;
        this.Zero_1471 = GeoCoordinate_73_Type.Value_22_Concept.Zero_1471;
        this.One_1472 = GeoCoordinate_73_Type.Value_22_Concept.One_1472;
        this.Default_1473 = GeoCoordinate_73_Type.Value_22_Concept.Default_1473;
        this.MinValue_1474 = GeoCoordinate_73_Type.Value_22_Concept.MinValue_1474;
        this.MaxValue_1475 = GeoCoordinate_73_Type.Value_22_Concept.MaxValue_1475;
        this.ToString_1477 = GeoCoordinate_73_Type.Value_22_Concept.ToString_1477;
    }
    // field accessors
    static Latitude_654 = function(self) { return self.Latitude_654; }
    static Longitude_658 = function(self) { return self.Longitude_658; }
    // implemented concepts 
    static Value_22_Concept = new Value_22_Concept(GeoCoordinate_73_Type);
    static Implements = [Value_22_Concept];
}
class GeoCoordinateWithAltitude_74_Type
{
    constructor(Coordinate_662, Altitude_666)
    {
        // field initialization 
        this.Coordinate_662 = Coordinate_662;
        this.Altitude_666 = Altitude_666;
        this.Type_1466 = GeoCoordinateWithAltitude_74_Type.Value_22_Concept.Type_1466;
        this.FieldTypes_1467 = GeoCoordinateWithAltitude_74_Type.Value_22_Concept.FieldTypes_1467;
        this.FieldNames_1468 = GeoCoordinateWithAltitude_74_Type.Value_22_Concept.FieldNames_1468;
        this.FieldValues_1470 = GeoCoordinateWithAltitude_74_Type.Value_22_Concept.FieldValues_1470;
        this.Zero_1471 = GeoCoordinateWithAltitude_74_Type.Value_22_Concept.Zero_1471;
        this.One_1472 = GeoCoordinateWithAltitude_74_Type.Value_22_Concept.One_1472;
        this.Default_1473 = GeoCoordinateWithAltitude_74_Type.Value_22_Concept.Default_1473;
        this.MinValue_1474 = GeoCoordinateWithAltitude_74_Type.Value_22_Concept.MinValue_1474;
        this.MaxValue_1475 = GeoCoordinateWithAltitude_74_Type.Value_22_Concept.MaxValue_1475;
        this.ToString_1477 = GeoCoordinateWithAltitude_74_Type.Value_22_Concept.ToString_1477;
    }
    // field accessors
    static Coordinate_662 = function(self) { return self.Coordinate_662; }
    static Altitude_666 = function(self) { return self.Altitude_666; }
    // implemented concepts 
    static Value_22_Concept = new Value_22_Concept(GeoCoordinateWithAltitude_74_Type);
    static Implements = [Value_22_Concept];
}
class Circle_75_Type
{
    constructor(Center_670, Radius_674)
    {
        // field initialization 
        this.Center_670 = Center_670;
        this.Radius_674 = Radius_674;
        this.Type_1466 = Circle_75_Type.Value_22_Concept.Type_1466;
        this.FieldTypes_1467 = Circle_75_Type.Value_22_Concept.FieldTypes_1467;
        this.FieldNames_1468 = Circle_75_Type.Value_22_Concept.FieldNames_1468;
        this.FieldValues_1470 = Circle_75_Type.Value_22_Concept.FieldValues_1470;
        this.Zero_1471 = Circle_75_Type.Value_22_Concept.Zero_1471;
        this.One_1472 = Circle_75_Type.Value_22_Concept.One_1472;
        this.Default_1473 = Circle_75_Type.Value_22_Concept.Default_1473;
        this.MinValue_1474 = Circle_75_Type.Value_22_Concept.MinValue_1474;
        this.MaxValue_1475 = Circle_75_Type.Value_22_Concept.MaxValue_1475;
        this.ToString_1477 = Circle_75_Type.Value_22_Concept.ToString_1477;
    }
    // field accessors
    static Center_670 = function(self) { return self.Center_670; }
    static Radius_674 = function(self) { return self.Radius_674; }
    // implemented concepts 
    static Value_22_Concept = new Value_22_Concept(Circle_75_Type);
    static Implements = [Value_22_Concept];
}
class Chord_76_Type
{
    constructor(Circle_678, Arc_682)
    {
        // field initialization 
        this.Circle_678 = Circle_678;
        this.Arc_682 = Arc_682;
        this.Type_1466 = Chord_76_Type.Value_22_Concept.Type_1466;
        this.FieldTypes_1467 = Chord_76_Type.Value_22_Concept.FieldTypes_1467;
        this.FieldNames_1468 = Chord_76_Type.Value_22_Concept.FieldNames_1468;
        this.FieldValues_1470 = Chord_76_Type.Value_22_Concept.FieldValues_1470;
        this.Zero_1471 = Chord_76_Type.Value_22_Concept.Zero_1471;
        this.One_1472 = Chord_76_Type.Value_22_Concept.One_1472;
        this.Default_1473 = Chord_76_Type.Value_22_Concept.Default_1473;
        this.MinValue_1474 = Chord_76_Type.Value_22_Concept.MinValue_1474;
        this.MaxValue_1475 = Chord_76_Type.Value_22_Concept.MaxValue_1475;
        this.ToString_1477 = Chord_76_Type.Value_22_Concept.ToString_1477;
    }
    // field accessors
    static Circle_678 = function(self) { return self.Circle_678; }
    static Arc_682 = function(self) { return self.Arc_682; }
    // implemented concepts 
    static Value_22_Concept = new Value_22_Concept(Chord_76_Type);
    static Implements = [Value_22_Concept];
}
class Size2D_77_Type
{
    constructor(Width_686, Height_690)
    {
        // field initialization 
        this.Width_686 = Width_686;
        this.Height_690 = Height_690;
        this.Type_1466 = Size2D_77_Type.Value_22_Concept.Type_1466;
        this.FieldTypes_1467 = Size2D_77_Type.Value_22_Concept.FieldTypes_1467;
        this.FieldNames_1468 = Size2D_77_Type.Value_22_Concept.FieldNames_1468;
        this.FieldValues_1470 = Size2D_77_Type.Value_22_Concept.FieldValues_1470;
        this.Zero_1471 = Size2D_77_Type.Value_22_Concept.Zero_1471;
        this.One_1472 = Size2D_77_Type.Value_22_Concept.One_1472;
        this.Default_1473 = Size2D_77_Type.Value_22_Concept.Default_1473;
        this.MinValue_1474 = Size2D_77_Type.Value_22_Concept.MinValue_1474;
        this.MaxValue_1475 = Size2D_77_Type.Value_22_Concept.MaxValue_1475;
        this.ToString_1477 = Size2D_77_Type.Value_22_Concept.ToString_1477;
    }
    // field accessors
    static Width_686 = function(self) { return self.Width_686; }
    static Height_690 = function(self) { return self.Height_690; }
    // implemented concepts 
    static Value_22_Concept = new Value_22_Concept(Size2D_77_Type);
    static Implements = [Value_22_Concept];
}
class Size3D_78_Type
{
    constructor(Width_694, Height_698, Depth_702)
    {
        // field initialization 
        this.Width_694 = Width_694;
        this.Height_698 = Height_698;
        this.Depth_702 = Depth_702;
        this.Type_1466 = Size3D_78_Type.Value_22_Concept.Type_1466;
        this.FieldTypes_1467 = Size3D_78_Type.Value_22_Concept.FieldTypes_1467;
        this.FieldNames_1468 = Size3D_78_Type.Value_22_Concept.FieldNames_1468;
        this.FieldValues_1470 = Size3D_78_Type.Value_22_Concept.FieldValues_1470;
        this.Zero_1471 = Size3D_78_Type.Value_22_Concept.Zero_1471;
        this.One_1472 = Size3D_78_Type.Value_22_Concept.One_1472;
        this.Default_1473 = Size3D_78_Type.Value_22_Concept.Default_1473;
        this.MinValue_1474 = Size3D_78_Type.Value_22_Concept.MinValue_1474;
        this.MaxValue_1475 = Size3D_78_Type.Value_22_Concept.MaxValue_1475;
        this.ToString_1477 = Size3D_78_Type.Value_22_Concept.ToString_1477;
    }
    // field accessors
    static Width_694 = function(self) { return self.Width_694; }
    static Height_698 = function(self) { return self.Height_698; }
    static Depth_702 = function(self) { return self.Depth_702; }
    // implemented concepts 
    static Value_22_Concept = new Value_22_Concept(Size3D_78_Type);
    static Implements = [Value_22_Concept];
}
class Rectangle2D_79_Type
{
    constructor(Center_706, Size_710)
    {
        // field initialization 
        this.Center_706 = Center_706;
        this.Size_710 = Size_710;
        this.Type_1466 = Rectangle2D_79_Type.Value_22_Concept.Type_1466;
        this.FieldTypes_1467 = Rectangle2D_79_Type.Value_22_Concept.FieldTypes_1467;
        this.FieldNames_1468 = Rectangle2D_79_Type.Value_22_Concept.FieldNames_1468;
        this.FieldValues_1470 = Rectangle2D_79_Type.Value_22_Concept.FieldValues_1470;
        this.Zero_1471 = Rectangle2D_79_Type.Value_22_Concept.Zero_1471;
        this.One_1472 = Rectangle2D_79_Type.Value_22_Concept.One_1472;
        this.Default_1473 = Rectangle2D_79_Type.Value_22_Concept.Default_1473;
        this.MinValue_1474 = Rectangle2D_79_Type.Value_22_Concept.MinValue_1474;
        this.MaxValue_1475 = Rectangle2D_79_Type.Value_22_Concept.MaxValue_1475;
        this.ToString_1477 = Rectangle2D_79_Type.Value_22_Concept.ToString_1477;
    }
    // field accessors
    static Center_706 = function(self) { return self.Center_706; }
    static Size_710 = function(self) { return self.Size_710; }
    // implemented concepts 
    static Value_22_Concept = new Value_22_Concept(Rectangle2D_79_Type);
    static Implements = [Value_22_Concept];
}
class Proportion_80_Type
{
    constructor(Value_714)
    {
        // field initialization 
        this.Value_714 = Value_714;
        this.Type_1466 = Proportion_80_Type.Value_22_Concept.Type_1466;
        this.FieldTypes_1467 = Proportion_80_Type.Value_22_Concept.FieldTypes_1467;
        this.FieldNames_1468 = Proportion_80_Type.Value_22_Concept.FieldNames_1468;
        this.FieldValues_1470 = Proportion_80_Type.Value_22_Concept.FieldValues_1470;
        this.Zero_1471 = Proportion_80_Type.Value_22_Concept.Zero_1471;
        this.One_1472 = Proportion_80_Type.Value_22_Concept.One_1472;
        this.Default_1473 = Proportion_80_Type.Value_22_Concept.Default_1473;
        this.MinValue_1474 = Proportion_80_Type.Value_22_Concept.MinValue_1474;
        this.MaxValue_1475 = Proportion_80_Type.Value_22_Concept.MaxValue_1475;
        this.ToString_1477 = Proportion_80_Type.Value_22_Concept.ToString_1477;
        this.Add_1428 = Proportion_80_Type.Arithmetic_19_Concept.Add_1428;
        this.Negative_1430 = Proportion_80_Type.Arithmetic_19_Concept.Negative_1430;
        this.Reciprocal_1432 = Proportion_80_Type.Arithmetic_19_Concept.Reciprocal_1432;
        this.Multiply_1435 = Proportion_80_Type.Arithmetic_19_Concept.Multiply_1435;
        this.Divide_1438 = Proportion_80_Type.Arithmetic_19_Concept.Divide_1438;
        this.Modulo_1441 = Proportion_80_Type.Arithmetic_19_Concept.Modulo_1441;
        this.Equals_1425 = Proportion_80_Type.Equatable_18_Concept.Equals_1425;
        this.Compare_1422 = Proportion_80_Type.Comparable_17_Concept.Compare_1422;
        this.Magnitude_1419 = Proportion_80_Type.Magnitude_16_Concept.Magnitude_1419;
    }
    // field accessors
    static Value_714 = function(self) { return self.Value_714; }
    // implemented concepts 
    static Value_22_Concept = new Value_22_Concept(Proportion_80_Type);
    static Arithmetic_19_Concept = new Arithmetic_19_Concept(Proportion_80_Type);
    static Equatable_18_Concept = new Equatable_18_Concept(Proportion_80_Type);
    static Comparable_17_Concept = new Comparable_17_Concept(Proportion_80_Type);
    static Magnitude_16_Concept = new Magnitude_16_Concept(Proportion_80_Type);
    static Numerical_15_Concept = new Numerical_15_Concept(Proportion_80_Type);
    static Implements = [Value_22_Concept,Arithmetic_19_Concept,Equatable_18_Concept,Comparable_17_Concept,Magnitude_16_Concept,Numerical_15_Concept];
}
class Fraction_81_Type
{
    constructor(Numerator_718, Denominator_722)
    {
        // field initialization 
        this.Numerator_718 = Numerator_718;
        this.Denominator_722 = Denominator_722;
        this.Type_1466 = Fraction_81_Type.Value_22_Concept.Type_1466;
        this.FieldTypes_1467 = Fraction_81_Type.Value_22_Concept.FieldTypes_1467;
        this.FieldNames_1468 = Fraction_81_Type.Value_22_Concept.FieldNames_1468;
        this.FieldValues_1470 = Fraction_81_Type.Value_22_Concept.FieldValues_1470;
        this.Zero_1471 = Fraction_81_Type.Value_22_Concept.Zero_1471;
        this.One_1472 = Fraction_81_Type.Value_22_Concept.One_1472;
        this.Default_1473 = Fraction_81_Type.Value_22_Concept.Default_1473;
        this.MinValue_1474 = Fraction_81_Type.Value_22_Concept.MinValue_1474;
        this.MaxValue_1475 = Fraction_81_Type.Value_22_Concept.MaxValue_1475;
        this.ToString_1477 = Fraction_81_Type.Value_22_Concept.ToString_1477;
    }
    // field accessors
    static Numerator_718 = function(self) { return self.Numerator_718; }
    static Denominator_722 = function(self) { return self.Denominator_722; }
    // implemented concepts 
    static Value_22_Concept = new Value_22_Concept(Fraction_81_Type);
    static Implements = [Value_22_Concept];
}
class Angle_82_Type
{
    constructor(Radians_726)
    {
        // field initialization 
        this.Radians_726 = Radians_726;
        this.Type_1466 = Angle_82_Type.Value_22_Concept.Type_1466;
        this.FieldTypes_1467 = Angle_82_Type.Value_22_Concept.FieldTypes_1467;
        this.FieldNames_1468 = Angle_82_Type.Value_22_Concept.FieldNames_1468;
        this.FieldValues_1470 = Angle_82_Type.Value_22_Concept.FieldValues_1470;
        this.Zero_1471 = Angle_82_Type.Value_22_Concept.Zero_1471;
        this.One_1472 = Angle_82_Type.Value_22_Concept.One_1472;
        this.Default_1473 = Angle_82_Type.Value_22_Concept.Default_1473;
        this.MinValue_1474 = Angle_82_Type.Value_22_Concept.MinValue_1474;
        this.MaxValue_1475 = Angle_82_Type.Value_22_Concept.MaxValue_1475;
        this.ToString_1477 = Angle_82_Type.Value_22_Concept.ToString_1477;
        this.Add_1445 = Angle_82_Type.ScalarArithmetic_20_Concept.Add_1445;
        this.Subtract_1448 = Angle_82_Type.ScalarArithmetic_20_Concept.Subtract_1448;
        this.Multiply_1451 = Angle_82_Type.ScalarArithmetic_20_Concept.Multiply_1451;
        this.Divide_1454 = Angle_82_Type.ScalarArithmetic_20_Concept.Divide_1454;
        this.Modulo_1457 = Angle_82_Type.ScalarArithmetic_20_Concept.Modulo_1457;
        this.Equals_1425 = Angle_82_Type.Equatable_18_Concept.Equals_1425;
        this.Compare_1422 = Angle_82_Type.Comparable_17_Concept.Compare_1422;
        this.Magnitude_1419 = Angle_82_Type.Magnitude_16_Concept.Magnitude_1419;
        this.Value_1417 = Angle_82_Type.Measure_14_Concept.Value_1417;
    }
    // field accessors
    static Radians_726 = function(self) { return self.Radians_726; }
    // implemented concepts 
    static Value_22_Concept = new Value_22_Concept(Angle_82_Type);
    static ScalarArithmetic_20_Concept = new ScalarArithmetic_20_Concept(Angle_82_Type);
    static Equatable_18_Concept = new Equatable_18_Concept(Angle_82_Type);
    static Comparable_17_Concept = new Comparable_17_Concept(Angle_82_Type);
    static Magnitude_16_Concept = new Magnitude_16_Concept(Angle_82_Type);
    static Measure_14_Concept = new Measure_14_Concept(Angle_82_Type);
    static Implements = [Value_22_Concept,ScalarArithmetic_20_Concept,Equatable_18_Concept,Comparable_17_Concept,Magnitude_16_Concept,Measure_14_Concept];
}
class Length_83_Type
{
    constructor(Meters_730)
    {
        // field initialization 
        this.Meters_730 = Meters_730;
        this.Type_1466 = Length_83_Type.Value_22_Concept.Type_1466;
        this.FieldTypes_1467 = Length_83_Type.Value_22_Concept.FieldTypes_1467;
        this.FieldNames_1468 = Length_83_Type.Value_22_Concept.FieldNames_1468;
        this.FieldValues_1470 = Length_83_Type.Value_22_Concept.FieldValues_1470;
        this.Zero_1471 = Length_83_Type.Value_22_Concept.Zero_1471;
        this.One_1472 = Length_83_Type.Value_22_Concept.One_1472;
        this.Default_1473 = Length_83_Type.Value_22_Concept.Default_1473;
        this.MinValue_1474 = Length_83_Type.Value_22_Concept.MinValue_1474;
        this.MaxValue_1475 = Length_83_Type.Value_22_Concept.MaxValue_1475;
        this.ToString_1477 = Length_83_Type.Value_22_Concept.ToString_1477;
        this.Add_1445 = Length_83_Type.ScalarArithmetic_20_Concept.Add_1445;
        this.Subtract_1448 = Length_83_Type.ScalarArithmetic_20_Concept.Subtract_1448;
        this.Multiply_1451 = Length_83_Type.ScalarArithmetic_20_Concept.Multiply_1451;
        this.Divide_1454 = Length_83_Type.ScalarArithmetic_20_Concept.Divide_1454;
        this.Modulo_1457 = Length_83_Type.ScalarArithmetic_20_Concept.Modulo_1457;
        this.Equals_1425 = Length_83_Type.Equatable_18_Concept.Equals_1425;
        this.Compare_1422 = Length_83_Type.Comparable_17_Concept.Compare_1422;
        this.Magnitude_1419 = Length_83_Type.Magnitude_16_Concept.Magnitude_1419;
        this.Value_1417 = Length_83_Type.Measure_14_Concept.Value_1417;
    }
    // field accessors
    static Meters_730 = function(self) { return self.Meters_730; }
    // implemented concepts 
    static Value_22_Concept = new Value_22_Concept(Length_83_Type);
    static ScalarArithmetic_20_Concept = new ScalarArithmetic_20_Concept(Length_83_Type);
    static Equatable_18_Concept = new Equatable_18_Concept(Length_83_Type);
    static Comparable_17_Concept = new Comparable_17_Concept(Length_83_Type);
    static Magnitude_16_Concept = new Magnitude_16_Concept(Length_83_Type);
    static Measure_14_Concept = new Measure_14_Concept(Length_83_Type);
    static Implements = [Value_22_Concept,ScalarArithmetic_20_Concept,Equatable_18_Concept,Comparable_17_Concept,Magnitude_16_Concept,Measure_14_Concept];
}
class Mass_84_Type
{
    constructor(Kilograms_734)
    {
        // field initialization 
        this.Kilograms_734 = Kilograms_734;
        this.Type_1466 = Mass_84_Type.Value_22_Concept.Type_1466;
        this.FieldTypes_1467 = Mass_84_Type.Value_22_Concept.FieldTypes_1467;
        this.FieldNames_1468 = Mass_84_Type.Value_22_Concept.FieldNames_1468;
        this.FieldValues_1470 = Mass_84_Type.Value_22_Concept.FieldValues_1470;
        this.Zero_1471 = Mass_84_Type.Value_22_Concept.Zero_1471;
        this.One_1472 = Mass_84_Type.Value_22_Concept.One_1472;
        this.Default_1473 = Mass_84_Type.Value_22_Concept.Default_1473;
        this.MinValue_1474 = Mass_84_Type.Value_22_Concept.MinValue_1474;
        this.MaxValue_1475 = Mass_84_Type.Value_22_Concept.MaxValue_1475;
        this.ToString_1477 = Mass_84_Type.Value_22_Concept.ToString_1477;
        this.Add_1445 = Mass_84_Type.ScalarArithmetic_20_Concept.Add_1445;
        this.Subtract_1448 = Mass_84_Type.ScalarArithmetic_20_Concept.Subtract_1448;
        this.Multiply_1451 = Mass_84_Type.ScalarArithmetic_20_Concept.Multiply_1451;
        this.Divide_1454 = Mass_84_Type.ScalarArithmetic_20_Concept.Divide_1454;
        this.Modulo_1457 = Mass_84_Type.ScalarArithmetic_20_Concept.Modulo_1457;
        this.Equals_1425 = Mass_84_Type.Equatable_18_Concept.Equals_1425;
        this.Compare_1422 = Mass_84_Type.Comparable_17_Concept.Compare_1422;
        this.Magnitude_1419 = Mass_84_Type.Magnitude_16_Concept.Magnitude_1419;
        this.Value_1417 = Mass_84_Type.Measure_14_Concept.Value_1417;
    }
    // field accessors
    static Kilograms_734 = function(self) { return self.Kilograms_734; }
    // implemented concepts 
    static Value_22_Concept = new Value_22_Concept(Mass_84_Type);
    static ScalarArithmetic_20_Concept = new ScalarArithmetic_20_Concept(Mass_84_Type);
    static Equatable_18_Concept = new Equatable_18_Concept(Mass_84_Type);
    static Comparable_17_Concept = new Comparable_17_Concept(Mass_84_Type);
    static Magnitude_16_Concept = new Magnitude_16_Concept(Mass_84_Type);
    static Measure_14_Concept = new Measure_14_Concept(Mass_84_Type);
    static Implements = [Value_22_Concept,ScalarArithmetic_20_Concept,Equatable_18_Concept,Comparable_17_Concept,Magnitude_16_Concept,Measure_14_Concept];
}
class Temperature_85_Type
{
    constructor(Celsius_738)
    {
        // field initialization 
        this.Celsius_738 = Celsius_738;
        this.Type_1466 = Temperature_85_Type.Value_22_Concept.Type_1466;
        this.FieldTypes_1467 = Temperature_85_Type.Value_22_Concept.FieldTypes_1467;
        this.FieldNames_1468 = Temperature_85_Type.Value_22_Concept.FieldNames_1468;
        this.FieldValues_1470 = Temperature_85_Type.Value_22_Concept.FieldValues_1470;
        this.Zero_1471 = Temperature_85_Type.Value_22_Concept.Zero_1471;
        this.One_1472 = Temperature_85_Type.Value_22_Concept.One_1472;
        this.Default_1473 = Temperature_85_Type.Value_22_Concept.Default_1473;
        this.MinValue_1474 = Temperature_85_Type.Value_22_Concept.MinValue_1474;
        this.MaxValue_1475 = Temperature_85_Type.Value_22_Concept.MaxValue_1475;
        this.ToString_1477 = Temperature_85_Type.Value_22_Concept.ToString_1477;
        this.Add_1445 = Temperature_85_Type.ScalarArithmetic_20_Concept.Add_1445;
        this.Subtract_1448 = Temperature_85_Type.ScalarArithmetic_20_Concept.Subtract_1448;
        this.Multiply_1451 = Temperature_85_Type.ScalarArithmetic_20_Concept.Multiply_1451;
        this.Divide_1454 = Temperature_85_Type.ScalarArithmetic_20_Concept.Divide_1454;
        this.Modulo_1457 = Temperature_85_Type.ScalarArithmetic_20_Concept.Modulo_1457;
        this.Equals_1425 = Temperature_85_Type.Equatable_18_Concept.Equals_1425;
        this.Compare_1422 = Temperature_85_Type.Comparable_17_Concept.Compare_1422;
        this.Magnitude_1419 = Temperature_85_Type.Magnitude_16_Concept.Magnitude_1419;
        this.Value_1417 = Temperature_85_Type.Measure_14_Concept.Value_1417;
    }
    // field accessors
    static Celsius_738 = function(self) { return self.Celsius_738; }
    // implemented concepts 
    static Value_22_Concept = new Value_22_Concept(Temperature_85_Type);
    static ScalarArithmetic_20_Concept = new ScalarArithmetic_20_Concept(Temperature_85_Type);
    static Equatable_18_Concept = new Equatable_18_Concept(Temperature_85_Type);
    static Comparable_17_Concept = new Comparable_17_Concept(Temperature_85_Type);
    static Magnitude_16_Concept = new Magnitude_16_Concept(Temperature_85_Type);
    static Measure_14_Concept = new Measure_14_Concept(Temperature_85_Type);
    static Implements = [Value_22_Concept,ScalarArithmetic_20_Concept,Equatable_18_Concept,Comparable_17_Concept,Magnitude_16_Concept,Measure_14_Concept];
}
class TimeSpan_86_Type
{
    constructor(Seconds_742)
    {
        // field initialization 
        this.Seconds_742 = Seconds_742;
        this.Type_1466 = TimeSpan_86_Type.Value_22_Concept.Type_1466;
        this.FieldTypes_1467 = TimeSpan_86_Type.Value_22_Concept.FieldTypes_1467;
        this.FieldNames_1468 = TimeSpan_86_Type.Value_22_Concept.FieldNames_1468;
        this.FieldValues_1470 = TimeSpan_86_Type.Value_22_Concept.FieldValues_1470;
        this.Zero_1471 = TimeSpan_86_Type.Value_22_Concept.Zero_1471;
        this.One_1472 = TimeSpan_86_Type.Value_22_Concept.One_1472;
        this.Default_1473 = TimeSpan_86_Type.Value_22_Concept.Default_1473;
        this.MinValue_1474 = TimeSpan_86_Type.Value_22_Concept.MinValue_1474;
        this.MaxValue_1475 = TimeSpan_86_Type.Value_22_Concept.MaxValue_1475;
        this.ToString_1477 = TimeSpan_86_Type.Value_22_Concept.ToString_1477;
        this.Add_1445 = TimeSpan_86_Type.ScalarArithmetic_20_Concept.Add_1445;
        this.Subtract_1448 = TimeSpan_86_Type.ScalarArithmetic_20_Concept.Subtract_1448;
        this.Multiply_1451 = TimeSpan_86_Type.ScalarArithmetic_20_Concept.Multiply_1451;
        this.Divide_1454 = TimeSpan_86_Type.ScalarArithmetic_20_Concept.Divide_1454;
        this.Modulo_1457 = TimeSpan_86_Type.ScalarArithmetic_20_Concept.Modulo_1457;
        this.Equals_1425 = TimeSpan_86_Type.Equatable_18_Concept.Equals_1425;
        this.Compare_1422 = TimeSpan_86_Type.Comparable_17_Concept.Compare_1422;
        this.Magnitude_1419 = TimeSpan_86_Type.Magnitude_16_Concept.Magnitude_1419;
        this.Value_1417 = TimeSpan_86_Type.Measure_14_Concept.Value_1417;
    }
    // field accessors
    static Seconds_742 = function(self) { return self.Seconds_742; }
    // implemented concepts 
    static Value_22_Concept = new Value_22_Concept(TimeSpan_86_Type);
    static ScalarArithmetic_20_Concept = new ScalarArithmetic_20_Concept(TimeSpan_86_Type);
    static Equatable_18_Concept = new Equatable_18_Concept(TimeSpan_86_Type);
    static Comparable_17_Concept = new Comparable_17_Concept(TimeSpan_86_Type);
    static Magnitude_16_Concept = new Magnitude_16_Concept(TimeSpan_86_Type);
    static Measure_14_Concept = new Measure_14_Concept(TimeSpan_86_Type);
    static Implements = [Value_22_Concept,ScalarArithmetic_20_Concept,Equatable_18_Concept,Comparable_17_Concept,Magnitude_16_Concept,Measure_14_Concept];
}
class TimeRange_87_Type
{
    constructor(Min_746, Max_750)
    {
        // field initialization 
        this.Min_746 = Min_746;
        this.Max_750 = Max_750;
        this.Count_1485 = TimeRange_87_Type.Array_24_Concept.Count_1485;
        this.At_1488 = TimeRange_87_Type.Array_24_Concept.At_1488;
        this.Type_1466 = TimeRange_87_Type.Value_22_Concept.Type_1466;
        this.FieldTypes_1467 = TimeRange_87_Type.Value_22_Concept.FieldTypes_1467;
        this.FieldNames_1468 = TimeRange_87_Type.Value_22_Concept.FieldNames_1468;
        this.FieldValues_1470 = TimeRange_87_Type.Value_22_Concept.FieldValues_1470;
        this.Zero_1471 = TimeRange_87_Type.Value_22_Concept.Zero_1471;
        this.One_1472 = TimeRange_87_Type.Value_22_Concept.One_1472;
        this.Default_1473 = TimeRange_87_Type.Value_22_Concept.Default_1473;
        this.MinValue_1474 = TimeRange_87_Type.Value_22_Concept.MinValue_1474;
        this.MaxValue_1475 = TimeRange_87_Type.Value_22_Concept.MaxValue_1475;
        this.ToString_1477 = TimeRange_87_Type.Value_22_Concept.ToString_1477;
        this.Add_1428 = TimeRange_87_Type.Arithmetic_19_Concept.Add_1428;
        this.Negative_1430 = TimeRange_87_Type.Arithmetic_19_Concept.Negative_1430;
        this.Reciprocal_1432 = TimeRange_87_Type.Arithmetic_19_Concept.Reciprocal_1432;
        this.Multiply_1435 = TimeRange_87_Type.Arithmetic_19_Concept.Multiply_1435;
        this.Divide_1438 = TimeRange_87_Type.Arithmetic_19_Concept.Divide_1438;
        this.Modulo_1441 = TimeRange_87_Type.Arithmetic_19_Concept.Modulo_1441;
        this.Equals_1425 = TimeRange_87_Type.Equatable_18_Concept.Equals_1425;
        this.Compare_1422 = TimeRange_87_Type.Comparable_17_Concept.Compare_1422;
        this.Magnitude_1419 = TimeRange_87_Type.Magnitude_16_Concept.Magnitude_1419;
        this.Count_1412 = TimeRange_87_Type.Vector_13_Concept.Count_1412;
        this.At_1415 = TimeRange_87_Type.Vector_13_Concept.At_1415;
        this.Min_1480 = TimeRange_87_Type.Interval_23_Concept.Min_1480;
        this.Max_1482 = TimeRange_87_Type.Interval_23_Concept.Max_1482;
    }
    // field accessors
    static Min_746 = function(self) { return self.Min_746; }
    static Max_750 = function(self) { return self.Max_750; }
    // implemented concepts 
    static Array_24_Concept = new Array_24_Concept(TimeRange_87_Type);
    static Value_22_Concept = new Value_22_Concept(TimeRange_87_Type);
    static Arithmetic_19_Concept = new Arithmetic_19_Concept(TimeRange_87_Type);
    static Equatable_18_Concept = new Equatable_18_Concept(TimeRange_87_Type);
    static Comparable_17_Concept = new Comparable_17_Concept(TimeRange_87_Type);
    static Magnitude_16_Concept = new Magnitude_16_Concept(TimeRange_87_Type);
    static Numerical_15_Concept = new Numerical_15_Concept(TimeRange_87_Type);
    static Vector_13_Concept = new Vector_13_Concept(TimeRange_87_Type);
    static Interval_23_Concept = new Interval_23_Concept(TimeRange_87_Type);
    static Implements = [Array_24_Concept,Value_22_Concept,Arithmetic_19_Concept,Equatable_18_Concept,Comparable_17_Concept,Magnitude_16_Concept,Numerical_15_Concept,Vector_13_Concept,Interval_23_Concept];
}
class DateTime_88_Type
{
    constructor()
    {
        // field initialization 
        this.Type_1466 = DateTime_88_Type.Value_22_Concept.Type_1466;
        this.FieldTypes_1467 = DateTime_88_Type.Value_22_Concept.FieldTypes_1467;
        this.FieldNames_1468 = DateTime_88_Type.Value_22_Concept.FieldNames_1468;
        this.FieldValues_1470 = DateTime_88_Type.Value_22_Concept.FieldValues_1470;
        this.Zero_1471 = DateTime_88_Type.Value_22_Concept.Zero_1471;
        this.One_1472 = DateTime_88_Type.Value_22_Concept.One_1472;
        this.Default_1473 = DateTime_88_Type.Value_22_Concept.Default_1473;
        this.MinValue_1474 = DateTime_88_Type.Value_22_Concept.MinValue_1474;
        this.MaxValue_1475 = DateTime_88_Type.Value_22_Concept.MaxValue_1475;
        this.ToString_1477 = DateTime_88_Type.Value_22_Concept.ToString_1477;
    }
    // field accessors
    // implemented concepts 
    static Value_22_Concept = new Value_22_Concept(DateTime_88_Type);
    static Implements = [Value_22_Concept];
}
class AnglePair_89_Type
{
    constructor(Start_754, End_758)
    {
        // field initialization 
        this.Start_754 = Start_754;
        this.End_758 = End_758;
        this.Count_1485 = AnglePair_89_Type.Array_24_Concept.Count_1485;
        this.At_1488 = AnglePair_89_Type.Array_24_Concept.At_1488;
        this.Type_1466 = AnglePair_89_Type.Value_22_Concept.Type_1466;
        this.FieldTypes_1467 = AnglePair_89_Type.Value_22_Concept.FieldTypes_1467;
        this.FieldNames_1468 = AnglePair_89_Type.Value_22_Concept.FieldNames_1468;
        this.FieldValues_1470 = AnglePair_89_Type.Value_22_Concept.FieldValues_1470;
        this.Zero_1471 = AnglePair_89_Type.Value_22_Concept.Zero_1471;
        this.One_1472 = AnglePair_89_Type.Value_22_Concept.One_1472;
        this.Default_1473 = AnglePair_89_Type.Value_22_Concept.Default_1473;
        this.MinValue_1474 = AnglePair_89_Type.Value_22_Concept.MinValue_1474;
        this.MaxValue_1475 = AnglePair_89_Type.Value_22_Concept.MaxValue_1475;
        this.ToString_1477 = AnglePair_89_Type.Value_22_Concept.ToString_1477;
        this.Add_1428 = AnglePair_89_Type.Arithmetic_19_Concept.Add_1428;
        this.Negative_1430 = AnglePair_89_Type.Arithmetic_19_Concept.Negative_1430;
        this.Reciprocal_1432 = AnglePair_89_Type.Arithmetic_19_Concept.Reciprocal_1432;
        this.Multiply_1435 = AnglePair_89_Type.Arithmetic_19_Concept.Multiply_1435;
        this.Divide_1438 = AnglePair_89_Type.Arithmetic_19_Concept.Divide_1438;
        this.Modulo_1441 = AnglePair_89_Type.Arithmetic_19_Concept.Modulo_1441;
        this.Equals_1425 = AnglePair_89_Type.Equatable_18_Concept.Equals_1425;
        this.Compare_1422 = AnglePair_89_Type.Comparable_17_Concept.Compare_1422;
        this.Magnitude_1419 = AnglePair_89_Type.Magnitude_16_Concept.Magnitude_1419;
        this.Count_1412 = AnglePair_89_Type.Vector_13_Concept.Count_1412;
        this.At_1415 = AnglePair_89_Type.Vector_13_Concept.At_1415;
        this.Min_1480 = AnglePair_89_Type.Interval_23_Concept.Min_1480;
        this.Max_1482 = AnglePair_89_Type.Interval_23_Concept.Max_1482;
    }
    // field accessors
    static Start_754 = function(self) { return self.Start_754; }
    static End_758 = function(self) { return self.End_758; }
    // implemented concepts 
    static Array_24_Concept = new Array_24_Concept(AnglePair_89_Type);
    static Value_22_Concept = new Value_22_Concept(AnglePair_89_Type);
    static Arithmetic_19_Concept = new Arithmetic_19_Concept(AnglePair_89_Type);
    static Equatable_18_Concept = new Equatable_18_Concept(AnglePair_89_Type);
    static Comparable_17_Concept = new Comparable_17_Concept(AnglePair_89_Type);
    static Magnitude_16_Concept = new Magnitude_16_Concept(AnglePair_89_Type);
    static Numerical_15_Concept = new Numerical_15_Concept(AnglePair_89_Type);
    static Vector_13_Concept = new Vector_13_Concept(AnglePair_89_Type);
    static Interval_23_Concept = new Interval_23_Concept(AnglePair_89_Type);
    static Implements = [Array_24_Concept,Value_22_Concept,Arithmetic_19_Concept,Equatable_18_Concept,Comparable_17_Concept,Magnitude_16_Concept,Numerical_15_Concept,Vector_13_Concept,Interval_23_Concept];
}
class Ring_90_Type
{
    constructor(Circle_762, InnerRadius_766)
    {
        // field initialization 
        this.Circle_762 = Circle_762;
        this.InnerRadius_766 = InnerRadius_766;
        this.Type_1466 = Ring_90_Type.Value_22_Concept.Type_1466;
        this.FieldTypes_1467 = Ring_90_Type.Value_22_Concept.FieldTypes_1467;
        this.FieldNames_1468 = Ring_90_Type.Value_22_Concept.FieldNames_1468;
        this.FieldValues_1470 = Ring_90_Type.Value_22_Concept.FieldValues_1470;
        this.Zero_1471 = Ring_90_Type.Value_22_Concept.Zero_1471;
        this.One_1472 = Ring_90_Type.Value_22_Concept.One_1472;
        this.Default_1473 = Ring_90_Type.Value_22_Concept.Default_1473;
        this.MinValue_1474 = Ring_90_Type.Value_22_Concept.MinValue_1474;
        this.MaxValue_1475 = Ring_90_Type.Value_22_Concept.MaxValue_1475;
        this.ToString_1477 = Ring_90_Type.Value_22_Concept.ToString_1477;
        this.Add_1428 = Ring_90_Type.Arithmetic_19_Concept.Add_1428;
        this.Negative_1430 = Ring_90_Type.Arithmetic_19_Concept.Negative_1430;
        this.Reciprocal_1432 = Ring_90_Type.Arithmetic_19_Concept.Reciprocal_1432;
        this.Multiply_1435 = Ring_90_Type.Arithmetic_19_Concept.Multiply_1435;
        this.Divide_1438 = Ring_90_Type.Arithmetic_19_Concept.Divide_1438;
        this.Modulo_1441 = Ring_90_Type.Arithmetic_19_Concept.Modulo_1441;
        this.Equals_1425 = Ring_90_Type.Equatable_18_Concept.Equals_1425;
        this.Compare_1422 = Ring_90_Type.Comparable_17_Concept.Compare_1422;
        this.Magnitude_1419 = Ring_90_Type.Magnitude_16_Concept.Magnitude_1419;
    }
    // field accessors
    static Circle_762 = function(self) { return self.Circle_762; }
    static InnerRadius_766 = function(self) { return self.InnerRadius_766; }
    // implemented concepts 
    static Value_22_Concept = new Value_22_Concept(Ring_90_Type);
    static Arithmetic_19_Concept = new Arithmetic_19_Concept(Ring_90_Type);
    static Equatable_18_Concept = new Equatable_18_Concept(Ring_90_Type);
    static Comparable_17_Concept = new Comparable_17_Concept(Ring_90_Type);
    static Magnitude_16_Concept = new Magnitude_16_Concept(Ring_90_Type);
    static Numerical_15_Concept = new Numerical_15_Concept(Ring_90_Type);
    static Implements = [Value_22_Concept,Arithmetic_19_Concept,Equatable_18_Concept,Comparable_17_Concept,Magnitude_16_Concept,Numerical_15_Concept];
}
class Arc_91_Type
{
    constructor(Angles_770, Cirlce_774)
    {
        // field initialization 
        this.Angles_770 = Angles_770;
        this.Cirlce_774 = Cirlce_774;
        this.Type_1466 = Arc_91_Type.Value_22_Concept.Type_1466;
        this.FieldTypes_1467 = Arc_91_Type.Value_22_Concept.FieldTypes_1467;
        this.FieldNames_1468 = Arc_91_Type.Value_22_Concept.FieldNames_1468;
        this.FieldValues_1470 = Arc_91_Type.Value_22_Concept.FieldValues_1470;
        this.Zero_1471 = Arc_91_Type.Value_22_Concept.Zero_1471;
        this.One_1472 = Arc_91_Type.Value_22_Concept.One_1472;
        this.Default_1473 = Arc_91_Type.Value_22_Concept.Default_1473;
        this.MinValue_1474 = Arc_91_Type.Value_22_Concept.MinValue_1474;
        this.MaxValue_1475 = Arc_91_Type.Value_22_Concept.MaxValue_1475;
        this.ToString_1477 = Arc_91_Type.Value_22_Concept.ToString_1477;
    }
    // field accessors
    static Angles_770 = function(self) { return self.Angles_770; }
    static Cirlce_774 = function(self) { return self.Cirlce_774; }
    // implemented concepts 
    static Value_22_Concept = new Value_22_Concept(Arc_91_Type);
    static Implements = [Value_22_Concept];
}
class TimeInterval_92_Type
{
    constructor(Start_778, End_782)
    {
        // field initialization 
        this.Start_778 = Start_778;
        this.End_782 = End_782;
        this.Count_1485 = TimeInterval_92_Type.Array_24_Concept.Count_1485;
        this.At_1488 = TimeInterval_92_Type.Array_24_Concept.At_1488;
        this.Type_1466 = TimeInterval_92_Type.Value_22_Concept.Type_1466;
        this.FieldTypes_1467 = TimeInterval_92_Type.Value_22_Concept.FieldTypes_1467;
        this.FieldNames_1468 = TimeInterval_92_Type.Value_22_Concept.FieldNames_1468;
        this.FieldValues_1470 = TimeInterval_92_Type.Value_22_Concept.FieldValues_1470;
        this.Zero_1471 = TimeInterval_92_Type.Value_22_Concept.Zero_1471;
        this.One_1472 = TimeInterval_92_Type.Value_22_Concept.One_1472;
        this.Default_1473 = TimeInterval_92_Type.Value_22_Concept.Default_1473;
        this.MinValue_1474 = TimeInterval_92_Type.Value_22_Concept.MinValue_1474;
        this.MaxValue_1475 = TimeInterval_92_Type.Value_22_Concept.MaxValue_1475;
        this.ToString_1477 = TimeInterval_92_Type.Value_22_Concept.ToString_1477;
        this.Add_1428 = TimeInterval_92_Type.Arithmetic_19_Concept.Add_1428;
        this.Negative_1430 = TimeInterval_92_Type.Arithmetic_19_Concept.Negative_1430;
        this.Reciprocal_1432 = TimeInterval_92_Type.Arithmetic_19_Concept.Reciprocal_1432;
        this.Multiply_1435 = TimeInterval_92_Type.Arithmetic_19_Concept.Multiply_1435;
        this.Divide_1438 = TimeInterval_92_Type.Arithmetic_19_Concept.Divide_1438;
        this.Modulo_1441 = TimeInterval_92_Type.Arithmetic_19_Concept.Modulo_1441;
        this.Equals_1425 = TimeInterval_92_Type.Equatable_18_Concept.Equals_1425;
        this.Compare_1422 = TimeInterval_92_Type.Comparable_17_Concept.Compare_1422;
        this.Magnitude_1419 = TimeInterval_92_Type.Magnitude_16_Concept.Magnitude_1419;
        this.Count_1412 = TimeInterval_92_Type.Vector_13_Concept.Count_1412;
        this.At_1415 = TimeInterval_92_Type.Vector_13_Concept.At_1415;
        this.Min_1480 = TimeInterval_92_Type.Interval_23_Concept.Min_1480;
        this.Max_1482 = TimeInterval_92_Type.Interval_23_Concept.Max_1482;
    }
    // field accessors
    static Start_778 = function(self) { return self.Start_778; }
    static End_782 = function(self) { return self.End_782; }
    // implemented concepts 
    static Array_24_Concept = new Array_24_Concept(TimeInterval_92_Type);
    static Value_22_Concept = new Value_22_Concept(TimeInterval_92_Type);
    static Arithmetic_19_Concept = new Arithmetic_19_Concept(TimeInterval_92_Type);
    static Equatable_18_Concept = new Equatable_18_Concept(TimeInterval_92_Type);
    static Comparable_17_Concept = new Comparable_17_Concept(TimeInterval_92_Type);
    static Magnitude_16_Concept = new Magnitude_16_Concept(TimeInterval_92_Type);
    static Numerical_15_Concept = new Numerical_15_Concept(TimeInterval_92_Type);
    static Vector_13_Concept = new Vector_13_Concept(TimeInterval_92_Type);
    static Interval_23_Concept = new Interval_23_Concept(TimeInterval_92_Type);
    static Implements = [Array_24_Concept,Value_22_Concept,Arithmetic_19_Concept,Equatable_18_Concept,Comparable_17_Concept,Magnitude_16_Concept,Numerical_15_Concept,Vector_13_Concept,Interval_23_Concept];
}
class RealInterval_93_Type
{
    constructor(A_786, B_790)
    {
        // field initialization 
        this.A_786 = A_786;
        this.B_790 = B_790;
        this.Count_1485 = RealInterval_93_Type.Array_24_Concept.Count_1485;
        this.At_1488 = RealInterval_93_Type.Array_24_Concept.At_1488;
        this.Type_1466 = RealInterval_93_Type.Value_22_Concept.Type_1466;
        this.FieldTypes_1467 = RealInterval_93_Type.Value_22_Concept.FieldTypes_1467;
        this.FieldNames_1468 = RealInterval_93_Type.Value_22_Concept.FieldNames_1468;
        this.FieldValues_1470 = RealInterval_93_Type.Value_22_Concept.FieldValues_1470;
        this.Zero_1471 = RealInterval_93_Type.Value_22_Concept.Zero_1471;
        this.One_1472 = RealInterval_93_Type.Value_22_Concept.One_1472;
        this.Default_1473 = RealInterval_93_Type.Value_22_Concept.Default_1473;
        this.MinValue_1474 = RealInterval_93_Type.Value_22_Concept.MinValue_1474;
        this.MaxValue_1475 = RealInterval_93_Type.Value_22_Concept.MaxValue_1475;
        this.ToString_1477 = RealInterval_93_Type.Value_22_Concept.ToString_1477;
        this.Add_1428 = RealInterval_93_Type.Arithmetic_19_Concept.Add_1428;
        this.Negative_1430 = RealInterval_93_Type.Arithmetic_19_Concept.Negative_1430;
        this.Reciprocal_1432 = RealInterval_93_Type.Arithmetic_19_Concept.Reciprocal_1432;
        this.Multiply_1435 = RealInterval_93_Type.Arithmetic_19_Concept.Multiply_1435;
        this.Divide_1438 = RealInterval_93_Type.Arithmetic_19_Concept.Divide_1438;
        this.Modulo_1441 = RealInterval_93_Type.Arithmetic_19_Concept.Modulo_1441;
        this.Equals_1425 = RealInterval_93_Type.Equatable_18_Concept.Equals_1425;
        this.Compare_1422 = RealInterval_93_Type.Comparable_17_Concept.Compare_1422;
        this.Magnitude_1419 = RealInterval_93_Type.Magnitude_16_Concept.Magnitude_1419;
        this.Count_1412 = RealInterval_93_Type.Vector_13_Concept.Count_1412;
        this.At_1415 = RealInterval_93_Type.Vector_13_Concept.At_1415;
        this.Min_1480 = RealInterval_93_Type.Interval_23_Concept.Min_1480;
        this.Max_1482 = RealInterval_93_Type.Interval_23_Concept.Max_1482;
    }
    // field accessors
    static A_786 = function(self) { return self.A_786; }
    static B_790 = function(self) { return self.B_790; }
    // implemented concepts 
    static Array_24_Concept = new Array_24_Concept(RealInterval_93_Type);
    static Value_22_Concept = new Value_22_Concept(RealInterval_93_Type);
    static Arithmetic_19_Concept = new Arithmetic_19_Concept(RealInterval_93_Type);
    static Equatable_18_Concept = new Equatable_18_Concept(RealInterval_93_Type);
    static Comparable_17_Concept = new Comparable_17_Concept(RealInterval_93_Type);
    static Magnitude_16_Concept = new Magnitude_16_Concept(RealInterval_93_Type);
    static Numerical_15_Concept = new Numerical_15_Concept(RealInterval_93_Type);
    static Vector_13_Concept = new Vector_13_Concept(RealInterval_93_Type);
    static Interval_23_Concept = new Interval_23_Concept(RealInterval_93_Type);
    static Implements = [Array_24_Concept,Value_22_Concept,Arithmetic_19_Concept,Equatable_18_Concept,Comparable_17_Concept,Magnitude_16_Concept,Numerical_15_Concept,Vector_13_Concept,Interval_23_Concept];
}
class Interval2D_94_Type
{
    constructor(A_794, B_798)
    {
        // field initialization 
        this.A_794 = A_794;
        this.B_798 = B_798;
        this.Count_1485 = Interval2D_94_Type.Array_24_Concept.Count_1485;
        this.At_1488 = Interval2D_94_Type.Array_24_Concept.At_1488;
        this.Type_1466 = Interval2D_94_Type.Value_22_Concept.Type_1466;
        this.FieldTypes_1467 = Interval2D_94_Type.Value_22_Concept.FieldTypes_1467;
        this.FieldNames_1468 = Interval2D_94_Type.Value_22_Concept.FieldNames_1468;
        this.FieldValues_1470 = Interval2D_94_Type.Value_22_Concept.FieldValues_1470;
        this.Zero_1471 = Interval2D_94_Type.Value_22_Concept.Zero_1471;
        this.One_1472 = Interval2D_94_Type.Value_22_Concept.One_1472;
        this.Default_1473 = Interval2D_94_Type.Value_22_Concept.Default_1473;
        this.MinValue_1474 = Interval2D_94_Type.Value_22_Concept.MinValue_1474;
        this.MaxValue_1475 = Interval2D_94_Type.Value_22_Concept.MaxValue_1475;
        this.ToString_1477 = Interval2D_94_Type.Value_22_Concept.ToString_1477;
        this.Add_1428 = Interval2D_94_Type.Arithmetic_19_Concept.Add_1428;
        this.Negative_1430 = Interval2D_94_Type.Arithmetic_19_Concept.Negative_1430;
        this.Reciprocal_1432 = Interval2D_94_Type.Arithmetic_19_Concept.Reciprocal_1432;
        this.Multiply_1435 = Interval2D_94_Type.Arithmetic_19_Concept.Multiply_1435;
        this.Divide_1438 = Interval2D_94_Type.Arithmetic_19_Concept.Divide_1438;
        this.Modulo_1441 = Interval2D_94_Type.Arithmetic_19_Concept.Modulo_1441;
        this.Equals_1425 = Interval2D_94_Type.Equatable_18_Concept.Equals_1425;
        this.Compare_1422 = Interval2D_94_Type.Comparable_17_Concept.Compare_1422;
        this.Magnitude_1419 = Interval2D_94_Type.Magnitude_16_Concept.Magnitude_1419;
        this.Count_1412 = Interval2D_94_Type.Vector_13_Concept.Count_1412;
        this.At_1415 = Interval2D_94_Type.Vector_13_Concept.At_1415;
        this.Min_1480 = Interval2D_94_Type.Interval_23_Concept.Min_1480;
        this.Max_1482 = Interval2D_94_Type.Interval_23_Concept.Max_1482;
    }
    // field accessors
    static A_794 = function(self) { return self.A_794; }
    static B_798 = function(self) { return self.B_798; }
    // implemented concepts 
    static Array_24_Concept = new Array_24_Concept(Interval2D_94_Type);
    static Value_22_Concept = new Value_22_Concept(Interval2D_94_Type);
    static Arithmetic_19_Concept = new Arithmetic_19_Concept(Interval2D_94_Type);
    static Equatable_18_Concept = new Equatable_18_Concept(Interval2D_94_Type);
    static Comparable_17_Concept = new Comparable_17_Concept(Interval2D_94_Type);
    static Magnitude_16_Concept = new Magnitude_16_Concept(Interval2D_94_Type);
    static Numerical_15_Concept = new Numerical_15_Concept(Interval2D_94_Type);
    static Vector_13_Concept = new Vector_13_Concept(Interval2D_94_Type);
    static Interval_23_Concept = new Interval_23_Concept(Interval2D_94_Type);
    static Implements = [Array_24_Concept,Value_22_Concept,Arithmetic_19_Concept,Equatable_18_Concept,Comparable_17_Concept,Magnitude_16_Concept,Numerical_15_Concept,Vector_13_Concept,Interval_23_Concept];
}
class Interval3D_95_Type
{
    constructor(A_802, B_806)
    {
        // field initialization 
        this.A_802 = A_802;
        this.B_806 = B_806;
        this.Count_1485 = Interval3D_95_Type.Array_24_Concept.Count_1485;
        this.At_1488 = Interval3D_95_Type.Array_24_Concept.At_1488;
        this.Type_1466 = Interval3D_95_Type.Value_22_Concept.Type_1466;
        this.FieldTypes_1467 = Interval3D_95_Type.Value_22_Concept.FieldTypes_1467;
        this.FieldNames_1468 = Interval3D_95_Type.Value_22_Concept.FieldNames_1468;
        this.FieldValues_1470 = Interval3D_95_Type.Value_22_Concept.FieldValues_1470;
        this.Zero_1471 = Interval3D_95_Type.Value_22_Concept.Zero_1471;
        this.One_1472 = Interval3D_95_Type.Value_22_Concept.One_1472;
        this.Default_1473 = Interval3D_95_Type.Value_22_Concept.Default_1473;
        this.MinValue_1474 = Interval3D_95_Type.Value_22_Concept.MinValue_1474;
        this.MaxValue_1475 = Interval3D_95_Type.Value_22_Concept.MaxValue_1475;
        this.ToString_1477 = Interval3D_95_Type.Value_22_Concept.ToString_1477;
        this.Add_1428 = Interval3D_95_Type.Arithmetic_19_Concept.Add_1428;
        this.Negative_1430 = Interval3D_95_Type.Arithmetic_19_Concept.Negative_1430;
        this.Reciprocal_1432 = Interval3D_95_Type.Arithmetic_19_Concept.Reciprocal_1432;
        this.Multiply_1435 = Interval3D_95_Type.Arithmetic_19_Concept.Multiply_1435;
        this.Divide_1438 = Interval3D_95_Type.Arithmetic_19_Concept.Divide_1438;
        this.Modulo_1441 = Interval3D_95_Type.Arithmetic_19_Concept.Modulo_1441;
        this.Equals_1425 = Interval3D_95_Type.Equatable_18_Concept.Equals_1425;
        this.Compare_1422 = Interval3D_95_Type.Comparable_17_Concept.Compare_1422;
        this.Magnitude_1419 = Interval3D_95_Type.Magnitude_16_Concept.Magnitude_1419;
        this.Count_1412 = Interval3D_95_Type.Vector_13_Concept.Count_1412;
        this.At_1415 = Interval3D_95_Type.Vector_13_Concept.At_1415;
        this.Min_1480 = Interval3D_95_Type.Interval_23_Concept.Min_1480;
        this.Max_1482 = Interval3D_95_Type.Interval_23_Concept.Max_1482;
    }
    // field accessors
    static A_802 = function(self) { return self.A_802; }
    static B_806 = function(self) { return self.B_806; }
    // implemented concepts 
    static Array_24_Concept = new Array_24_Concept(Interval3D_95_Type);
    static Value_22_Concept = new Value_22_Concept(Interval3D_95_Type);
    static Arithmetic_19_Concept = new Arithmetic_19_Concept(Interval3D_95_Type);
    static Equatable_18_Concept = new Equatable_18_Concept(Interval3D_95_Type);
    static Comparable_17_Concept = new Comparable_17_Concept(Interval3D_95_Type);
    static Magnitude_16_Concept = new Magnitude_16_Concept(Interval3D_95_Type);
    static Numerical_15_Concept = new Numerical_15_Concept(Interval3D_95_Type);
    static Vector_13_Concept = new Vector_13_Concept(Interval3D_95_Type);
    static Interval_23_Concept = new Interval_23_Concept(Interval3D_95_Type);
    static Implements = [Array_24_Concept,Value_22_Concept,Arithmetic_19_Concept,Equatable_18_Concept,Comparable_17_Concept,Magnitude_16_Concept,Numerical_15_Concept,Vector_13_Concept,Interval_23_Concept];
}
class Capsule_96_Type
{
    constructor(Line_810, Radius_814)
    {
        // field initialization 
        this.Line_810 = Line_810;
        this.Radius_814 = Radius_814;
        this.Type_1466 = Capsule_96_Type.Value_22_Concept.Type_1466;
        this.FieldTypes_1467 = Capsule_96_Type.Value_22_Concept.FieldTypes_1467;
        this.FieldNames_1468 = Capsule_96_Type.Value_22_Concept.FieldNames_1468;
        this.FieldValues_1470 = Capsule_96_Type.Value_22_Concept.FieldValues_1470;
        this.Zero_1471 = Capsule_96_Type.Value_22_Concept.Zero_1471;
        this.One_1472 = Capsule_96_Type.Value_22_Concept.One_1472;
        this.Default_1473 = Capsule_96_Type.Value_22_Concept.Default_1473;
        this.MinValue_1474 = Capsule_96_Type.Value_22_Concept.MinValue_1474;
        this.MaxValue_1475 = Capsule_96_Type.Value_22_Concept.MaxValue_1475;
        this.ToString_1477 = Capsule_96_Type.Value_22_Concept.ToString_1477;
    }
    // field accessors
    static Line_810 = function(self) { return self.Line_810; }
    static Radius_814 = function(self) { return self.Radius_814; }
    // implemented concepts 
    static Value_22_Concept = new Value_22_Concept(Capsule_96_Type);
    static Implements = [Value_22_Concept];
}
class Matrix3D_97_Type
{
    constructor(Column1_818, Column2_822, Column3_826, Column4_830)
    {
        // field initialization 
        this.Column1_818 = Column1_818;
        this.Column2_822 = Column2_822;
        this.Column3_826 = Column3_826;
        this.Column4_830 = Column4_830;
        this.Type_1466 = Matrix3D_97_Type.Value_22_Concept.Type_1466;
        this.FieldTypes_1467 = Matrix3D_97_Type.Value_22_Concept.FieldTypes_1467;
        this.FieldNames_1468 = Matrix3D_97_Type.Value_22_Concept.FieldNames_1468;
        this.FieldValues_1470 = Matrix3D_97_Type.Value_22_Concept.FieldValues_1470;
        this.Zero_1471 = Matrix3D_97_Type.Value_22_Concept.Zero_1471;
        this.One_1472 = Matrix3D_97_Type.Value_22_Concept.One_1472;
        this.Default_1473 = Matrix3D_97_Type.Value_22_Concept.Default_1473;
        this.MinValue_1474 = Matrix3D_97_Type.Value_22_Concept.MinValue_1474;
        this.MaxValue_1475 = Matrix3D_97_Type.Value_22_Concept.MaxValue_1475;
        this.ToString_1477 = Matrix3D_97_Type.Value_22_Concept.ToString_1477;
    }
    // field accessors
    static Column1_818 = function(self) { return self.Column1_818; }
    static Column2_822 = function(self) { return self.Column2_822; }
    static Column3_826 = function(self) { return self.Column3_826; }
    static Column4_830 = function(self) { return self.Column4_830; }
    // implemented concepts 
    static Value_22_Concept = new Value_22_Concept(Matrix3D_97_Type);
    static Implements = [Value_22_Concept];
}
class Cylinder_98_Type
{
    constructor(Line_834, Radius_838)
    {
        // field initialization 
        this.Line_834 = Line_834;
        this.Radius_838 = Radius_838;
        this.Type_1466 = Cylinder_98_Type.Value_22_Concept.Type_1466;
        this.FieldTypes_1467 = Cylinder_98_Type.Value_22_Concept.FieldTypes_1467;
        this.FieldNames_1468 = Cylinder_98_Type.Value_22_Concept.FieldNames_1468;
        this.FieldValues_1470 = Cylinder_98_Type.Value_22_Concept.FieldValues_1470;
        this.Zero_1471 = Cylinder_98_Type.Value_22_Concept.Zero_1471;
        this.One_1472 = Cylinder_98_Type.Value_22_Concept.One_1472;
        this.Default_1473 = Cylinder_98_Type.Value_22_Concept.Default_1473;
        this.MinValue_1474 = Cylinder_98_Type.Value_22_Concept.MinValue_1474;
        this.MaxValue_1475 = Cylinder_98_Type.Value_22_Concept.MaxValue_1475;
        this.ToString_1477 = Cylinder_98_Type.Value_22_Concept.ToString_1477;
    }
    // field accessors
    static Line_834 = function(self) { return self.Line_834; }
    static Radius_838 = function(self) { return self.Radius_838; }
    // implemented concepts 
    static Value_22_Concept = new Value_22_Concept(Cylinder_98_Type);
    static Implements = [Value_22_Concept];
}
class Cone_99_Type
{
    constructor(Line_842, Radius_846)
    {
        // field initialization 
        this.Line_842 = Line_842;
        this.Radius_846 = Radius_846;
        this.Type_1466 = Cone_99_Type.Value_22_Concept.Type_1466;
        this.FieldTypes_1467 = Cone_99_Type.Value_22_Concept.FieldTypes_1467;
        this.FieldNames_1468 = Cone_99_Type.Value_22_Concept.FieldNames_1468;
        this.FieldValues_1470 = Cone_99_Type.Value_22_Concept.FieldValues_1470;
        this.Zero_1471 = Cone_99_Type.Value_22_Concept.Zero_1471;
        this.One_1472 = Cone_99_Type.Value_22_Concept.One_1472;
        this.Default_1473 = Cone_99_Type.Value_22_Concept.Default_1473;
        this.MinValue_1474 = Cone_99_Type.Value_22_Concept.MinValue_1474;
        this.MaxValue_1475 = Cone_99_Type.Value_22_Concept.MaxValue_1475;
        this.ToString_1477 = Cone_99_Type.Value_22_Concept.ToString_1477;
    }
    // field accessors
    static Line_842 = function(self) { return self.Line_842; }
    static Radius_846 = function(self) { return self.Radius_846; }
    // implemented concepts 
    static Value_22_Concept = new Value_22_Concept(Cone_99_Type);
    static Implements = [Value_22_Concept];
}
class Tube_100_Type
{
    constructor(Line_850, InnerRadius_854, OuterRadius_858)
    {
        // field initialization 
        this.Line_850 = Line_850;
        this.InnerRadius_854 = InnerRadius_854;
        this.OuterRadius_858 = OuterRadius_858;
        this.Type_1466 = Tube_100_Type.Value_22_Concept.Type_1466;
        this.FieldTypes_1467 = Tube_100_Type.Value_22_Concept.FieldTypes_1467;
        this.FieldNames_1468 = Tube_100_Type.Value_22_Concept.FieldNames_1468;
        this.FieldValues_1470 = Tube_100_Type.Value_22_Concept.FieldValues_1470;
        this.Zero_1471 = Tube_100_Type.Value_22_Concept.Zero_1471;
        this.One_1472 = Tube_100_Type.Value_22_Concept.One_1472;
        this.Default_1473 = Tube_100_Type.Value_22_Concept.Default_1473;
        this.MinValue_1474 = Tube_100_Type.Value_22_Concept.MinValue_1474;
        this.MaxValue_1475 = Tube_100_Type.Value_22_Concept.MaxValue_1475;
        this.ToString_1477 = Tube_100_Type.Value_22_Concept.ToString_1477;
    }
    // field accessors
    static Line_850 = function(self) { return self.Line_850; }
    static InnerRadius_854 = function(self) { return self.InnerRadius_854; }
    static OuterRadius_858 = function(self) { return self.OuterRadius_858; }
    // implemented concepts 
    static Value_22_Concept = new Value_22_Concept(Tube_100_Type);
    static Implements = [Value_22_Concept];
}
class ConeSegment_101_Type
{
    constructor(Line_862, Radius1_866, Radius2_870)
    {
        // field initialization 
        this.Line_862 = Line_862;
        this.Radius1_866 = Radius1_866;
        this.Radius2_870 = Radius2_870;
        this.Type_1466 = ConeSegment_101_Type.Value_22_Concept.Type_1466;
        this.FieldTypes_1467 = ConeSegment_101_Type.Value_22_Concept.FieldTypes_1467;
        this.FieldNames_1468 = ConeSegment_101_Type.Value_22_Concept.FieldNames_1468;
        this.FieldValues_1470 = ConeSegment_101_Type.Value_22_Concept.FieldValues_1470;
        this.Zero_1471 = ConeSegment_101_Type.Value_22_Concept.Zero_1471;
        this.One_1472 = ConeSegment_101_Type.Value_22_Concept.One_1472;
        this.Default_1473 = ConeSegment_101_Type.Value_22_Concept.Default_1473;
        this.MinValue_1474 = ConeSegment_101_Type.Value_22_Concept.MinValue_1474;
        this.MaxValue_1475 = ConeSegment_101_Type.Value_22_Concept.MaxValue_1475;
        this.ToString_1477 = ConeSegment_101_Type.Value_22_Concept.ToString_1477;
    }
    // field accessors
    static Line_862 = function(self) { return self.Line_862; }
    static Radius1_866 = function(self) { return self.Radius1_866; }
    static Radius2_870 = function(self) { return self.Radius2_870; }
    // implemented concepts 
    static Value_22_Concept = new Value_22_Concept(ConeSegment_101_Type);
    static Implements = [Value_22_Concept];
}
class Box2D_102_Type
{
    constructor(Center_874, Rotation_878, Extent_882)
    {
        // field initialization 
        this.Center_874 = Center_874;
        this.Rotation_878 = Rotation_878;
        this.Extent_882 = Extent_882;
        this.Type_1466 = Box2D_102_Type.Value_22_Concept.Type_1466;
        this.FieldTypes_1467 = Box2D_102_Type.Value_22_Concept.FieldTypes_1467;
        this.FieldNames_1468 = Box2D_102_Type.Value_22_Concept.FieldNames_1468;
        this.FieldValues_1470 = Box2D_102_Type.Value_22_Concept.FieldValues_1470;
        this.Zero_1471 = Box2D_102_Type.Value_22_Concept.Zero_1471;
        this.One_1472 = Box2D_102_Type.Value_22_Concept.One_1472;
        this.Default_1473 = Box2D_102_Type.Value_22_Concept.Default_1473;
        this.MinValue_1474 = Box2D_102_Type.Value_22_Concept.MinValue_1474;
        this.MaxValue_1475 = Box2D_102_Type.Value_22_Concept.MaxValue_1475;
        this.ToString_1477 = Box2D_102_Type.Value_22_Concept.ToString_1477;
    }
    // field accessors
    static Center_874 = function(self) { return self.Center_874; }
    static Rotation_878 = function(self) { return self.Rotation_878; }
    static Extent_882 = function(self) { return self.Extent_882; }
    // implemented concepts 
    static Value_22_Concept = new Value_22_Concept(Box2D_102_Type);
    static Implements = [Value_22_Concept];
}
class Box3D_103_Type
{
    constructor(Center_886, Rotation_890, Extent_894)
    {
        // field initialization 
        this.Center_886 = Center_886;
        this.Rotation_890 = Rotation_890;
        this.Extent_894 = Extent_894;
        this.Type_1466 = Box3D_103_Type.Value_22_Concept.Type_1466;
        this.FieldTypes_1467 = Box3D_103_Type.Value_22_Concept.FieldTypes_1467;
        this.FieldNames_1468 = Box3D_103_Type.Value_22_Concept.FieldNames_1468;
        this.FieldValues_1470 = Box3D_103_Type.Value_22_Concept.FieldValues_1470;
        this.Zero_1471 = Box3D_103_Type.Value_22_Concept.Zero_1471;
        this.One_1472 = Box3D_103_Type.Value_22_Concept.One_1472;
        this.Default_1473 = Box3D_103_Type.Value_22_Concept.Default_1473;
        this.MinValue_1474 = Box3D_103_Type.Value_22_Concept.MinValue_1474;
        this.MaxValue_1475 = Box3D_103_Type.Value_22_Concept.MaxValue_1475;
        this.ToString_1477 = Box3D_103_Type.Value_22_Concept.ToString_1477;
    }
    // field accessors
    static Center_886 = function(self) { return self.Center_886; }
    static Rotation_890 = function(self) { return self.Rotation_890; }
    static Extent_894 = function(self) { return self.Extent_894; }
    // implemented concepts 
    static Value_22_Concept = new Value_22_Concept(Box3D_103_Type);
    static Implements = [Value_22_Concept];
}
class CubicBezierTriangle3D_104_Type
{
    constructor(A_898, B_902, C_906, A2B_910, AB2_914, B2C_918, BC2_922, AC2_926, A2C_930, ABC_934)
    {
        // field initialization 
        this.A_898 = A_898;
        this.B_902 = B_902;
        this.C_906 = C_906;
        this.A2B_910 = A2B_910;
        this.AB2_914 = AB2_914;
        this.B2C_918 = B2C_918;
        this.BC2_922 = BC2_922;
        this.AC2_926 = AC2_926;
        this.A2C_930 = A2C_930;
        this.ABC_934 = ABC_934;
        this.Type_1466 = CubicBezierTriangle3D_104_Type.Value_22_Concept.Type_1466;
        this.FieldTypes_1467 = CubicBezierTriangle3D_104_Type.Value_22_Concept.FieldTypes_1467;
        this.FieldNames_1468 = CubicBezierTriangle3D_104_Type.Value_22_Concept.FieldNames_1468;
        this.FieldValues_1470 = CubicBezierTriangle3D_104_Type.Value_22_Concept.FieldValues_1470;
        this.Zero_1471 = CubicBezierTriangle3D_104_Type.Value_22_Concept.Zero_1471;
        this.One_1472 = CubicBezierTriangle3D_104_Type.Value_22_Concept.One_1472;
        this.Default_1473 = CubicBezierTriangle3D_104_Type.Value_22_Concept.Default_1473;
        this.MinValue_1474 = CubicBezierTriangle3D_104_Type.Value_22_Concept.MinValue_1474;
        this.MaxValue_1475 = CubicBezierTriangle3D_104_Type.Value_22_Concept.MaxValue_1475;
        this.ToString_1477 = CubicBezierTriangle3D_104_Type.Value_22_Concept.ToString_1477;
    }
    // field accessors
    static A_898 = function(self) { return self.A_898; }
    static B_902 = function(self) { return self.B_902; }
    static C_906 = function(self) { return self.C_906; }
    static A2B_910 = function(self) { return self.A2B_910; }
    static AB2_914 = function(self) { return self.AB2_914; }
    static B2C_918 = function(self) { return self.B2C_918; }
    static BC2_922 = function(self) { return self.BC2_922; }
    static AC2_926 = function(self) { return self.AC2_926; }
    static A2C_930 = function(self) { return self.A2C_930; }
    static ABC_934 = function(self) { return self.ABC_934; }
    // implemented concepts 
    static Value_22_Concept = new Value_22_Concept(CubicBezierTriangle3D_104_Type);
    static Implements = [Value_22_Concept];
}
class CubicBezier2D_105_Type
{
    constructor(A_938, B_942, C_946, D_950)
    {
        // field initialization 
        this.A_938 = A_938;
        this.B_942 = B_942;
        this.C_946 = C_946;
        this.D_950 = D_950;
        this.Type_1466 = CubicBezier2D_105_Type.Value_22_Concept.Type_1466;
        this.FieldTypes_1467 = CubicBezier2D_105_Type.Value_22_Concept.FieldTypes_1467;
        this.FieldNames_1468 = CubicBezier2D_105_Type.Value_22_Concept.FieldNames_1468;
        this.FieldValues_1470 = CubicBezier2D_105_Type.Value_22_Concept.FieldValues_1470;
        this.Zero_1471 = CubicBezier2D_105_Type.Value_22_Concept.Zero_1471;
        this.One_1472 = CubicBezier2D_105_Type.Value_22_Concept.One_1472;
        this.Default_1473 = CubicBezier2D_105_Type.Value_22_Concept.Default_1473;
        this.MinValue_1474 = CubicBezier2D_105_Type.Value_22_Concept.MinValue_1474;
        this.MaxValue_1475 = CubicBezier2D_105_Type.Value_22_Concept.MaxValue_1475;
        this.ToString_1477 = CubicBezier2D_105_Type.Value_22_Concept.ToString_1477;
    }
    // field accessors
    static A_938 = function(self) { return self.A_938; }
    static B_942 = function(self) { return self.B_942; }
    static C_946 = function(self) { return self.C_946; }
    static D_950 = function(self) { return self.D_950; }
    // implemented concepts 
    static Value_22_Concept = new Value_22_Concept(CubicBezier2D_105_Type);
    static Implements = [Value_22_Concept];
}
class UV_106_Type
{
    constructor(U_954, V_958)
    {
        // field initialization 
        this.U_954 = U_954;
        this.V_958 = V_958;
        this.Count_1485 = UV_106_Type.Array_24_Concept.Count_1485;
        this.At_1488 = UV_106_Type.Array_24_Concept.At_1488;
        this.Type_1466 = UV_106_Type.Value_22_Concept.Type_1466;
        this.FieldTypes_1467 = UV_106_Type.Value_22_Concept.FieldTypes_1467;
        this.FieldNames_1468 = UV_106_Type.Value_22_Concept.FieldNames_1468;
        this.FieldValues_1470 = UV_106_Type.Value_22_Concept.FieldValues_1470;
        this.Zero_1471 = UV_106_Type.Value_22_Concept.Zero_1471;
        this.One_1472 = UV_106_Type.Value_22_Concept.One_1472;
        this.Default_1473 = UV_106_Type.Value_22_Concept.Default_1473;
        this.MinValue_1474 = UV_106_Type.Value_22_Concept.MinValue_1474;
        this.MaxValue_1475 = UV_106_Type.Value_22_Concept.MaxValue_1475;
        this.ToString_1477 = UV_106_Type.Value_22_Concept.ToString_1477;
        this.Add_1428 = UV_106_Type.Arithmetic_19_Concept.Add_1428;
        this.Negative_1430 = UV_106_Type.Arithmetic_19_Concept.Negative_1430;
        this.Reciprocal_1432 = UV_106_Type.Arithmetic_19_Concept.Reciprocal_1432;
        this.Multiply_1435 = UV_106_Type.Arithmetic_19_Concept.Multiply_1435;
        this.Divide_1438 = UV_106_Type.Arithmetic_19_Concept.Divide_1438;
        this.Modulo_1441 = UV_106_Type.Arithmetic_19_Concept.Modulo_1441;
        this.Equals_1425 = UV_106_Type.Equatable_18_Concept.Equals_1425;
        this.Compare_1422 = UV_106_Type.Comparable_17_Concept.Compare_1422;
        this.Magnitude_1419 = UV_106_Type.Magnitude_16_Concept.Magnitude_1419;
        this.Count_1412 = UV_106_Type.Vector_13_Concept.Count_1412;
        this.At_1415 = UV_106_Type.Vector_13_Concept.At_1415;
    }
    // field accessors
    static U_954 = function(self) { return self.U_954; }
    static V_958 = function(self) { return self.V_958; }
    // implemented concepts 
    static Array_24_Concept = new Array_24_Concept(UV_106_Type);
    static Value_22_Concept = new Value_22_Concept(UV_106_Type);
    static Arithmetic_19_Concept = new Arithmetic_19_Concept(UV_106_Type);
    static Equatable_18_Concept = new Equatable_18_Concept(UV_106_Type);
    static Comparable_17_Concept = new Comparable_17_Concept(UV_106_Type);
    static Magnitude_16_Concept = new Magnitude_16_Concept(UV_106_Type);
    static Numerical_15_Concept = new Numerical_15_Concept(UV_106_Type);
    static Vector_13_Concept = new Vector_13_Concept(UV_106_Type);
    static Implements = [Array_24_Concept,Value_22_Concept,Arithmetic_19_Concept,Equatable_18_Concept,Comparable_17_Concept,Magnitude_16_Concept,Numerical_15_Concept,Vector_13_Concept];
}
class UVW_107_Type
{
    constructor(U_962, V_966, W_970)
    {
        // field initialization 
        this.U_962 = U_962;
        this.V_966 = V_966;
        this.W_970 = W_970;
        this.Count_1485 = UVW_107_Type.Array_24_Concept.Count_1485;
        this.At_1488 = UVW_107_Type.Array_24_Concept.At_1488;
        this.Type_1466 = UVW_107_Type.Value_22_Concept.Type_1466;
        this.FieldTypes_1467 = UVW_107_Type.Value_22_Concept.FieldTypes_1467;
        this.FieldNames_1468 = UVW_107_Type.Value_22_Concept.FieldNames_1468;
        this.FieldValues_1470 = UVW_107_Type.Value_22_Concept.FieldValues_1470;
        this.Zero_1471 = UVW_107_Type.Value_22_Concept.Zero_1471;
        this.One_1472 = UVW_107_Type.Value_22_Concept.One_1472;
        this.Default_1473 = UVW_107_Type.Value_22_Concept.Default_1473;
        this.MinValue_1474 = UVW_107_Type.Value_22_Concept.MinValue_1474;
        this.MaxValue_1475 = UVW_107_Type.Value_22_Concept.MaxValue_1475;
        this.ToString_1477 = UVW_107_Type.Value_22_Concept.ToString_1477;
        this.Add_1428 = UVW_107_Type.Arithmetic_19_Concept.Add_1428;
        this.Negative_1430 = UVW_107_Type.Arithmetic_19_Concept.Negative_1430;
        this.Reciprocal_1432 = UVW_107_Type.Arithmetic_19_Concept.Reciprocal_1432;
        this.Multiply_1435 = UVW_107_Type.Arithmetic_19_Concept.Multiply_1435;
        this.Divide_1438 = UVW_107_Type.Arithmetic_19_Concept.Divide_1438;
        this.Modulo_1441 = UVW_107_Type.Arithmetic_19_Concept.Modulo_1441;
        this.Equals_1425 = UVW_107_Type.Equatable_18_Concept.Equals_1425;
        this.Compare_1422 = UVW_107_Type.Comparable_17_Concept.Compare_1422;
        this.Magnitude_1419 = UVW_107_Type.Magnitude_16_Concept.Magnitude_1419;
        this.Count_1412 = UVW_107_Type.Vector_13_Concept.Count_1412;
        this.At_1415 = UVW_107_Type.Vector_13_Concept.At_1415;
    }
    // field accessors
    static U_962 = function(self) { return self.U_962; }
    static V_966 = function(self) { return self.V_966; }
    static W_970 = function(self) { return self.W_970; }
    // implemented concepts 
    static Array_24_Concept = new Array_24_Concept(UVW_107_Type);
    static Value_22_Concept = new Value_22_Concept(UVW_107_Type);
    static Arithmetic_19_Concept = new Arithmetic_19_Concept(UVW_107_Type);
    static Equatable_18_Concept = new Equatable_18_Concept(UVW_107_Type);
    static Comparable_17_Concept = new Comparable_17_Concept(UVW_107_Type);
    static Magnitude_16_Concept = new Magnitude_16_Concept(UVW_107_Type);
    static Numerical_15_Concept = new Numerical_15_Concept(UVW_107_Type);
    static Vector_13_Concept = new Vector_13_Concept(UVW_107_Type);
    static Implements = [Array_24_Concept,Value_22_Concept,Arithmetic_19_Concept,Equatable_18_Concept,Comparable_17_Concept,Magnitude_16_Concept,Numerical_15_Concept,Vector_13_Concept];
}
class CubicBezier3D_108_Type
{
    constructor(A_974, B_978, C_982, D_986)
    {
        // field initialization 
        this.A_974 = A_974;
        this.B_978 = B_978;
        this.C_982 = C_982;
        this.D_986 = D_986;
        this.Type_1466 = CubicBezier3D_108_Type.Value_22_Concept.Type_1466;
        this.FieldTypes_1467 = CubicBezier3D_108_Type.Value_22_Concept.FieldTypes_1467;
        this.FieldNames_1468 = CubicBezier3D_108_Type.Value_22_Concept.FieldNames_1468;
        this.FieldValues_1470 = CubicBezier3D_108_Type.Value_22_Concept.FieldValues_1470;
        this.Zero_1471 = CubicBezier3D_108_Type.Value_22_Concept.Zero_1471;
        this.One_1472 = CubicBezier3D_108_Type.Value_22_Concept.One_1472;
        this.Default_1473 = CubicBezier3D_108_Type.Value_22_Concept.Default_1473;
        this.MinValue_1474 = CubicBezier3D_108_Type.Value_22_Concept.MinValue_1474;
        this.MaxValue_1475 = CubicBezier3D_108_Type.Value_22_Concept.MaxValue_1475;
        this.ToString_1477 = CubicBezier3D_108_Type.Value_22_Concept.ToString_1477;
    }
    // field accessors
    static A_974 = function(self) { return self.A_974; }
    static B_978 = function(self) { return self.B_978; }
    static C_982 = function(self) { return self.C_982; }
    static D_986 = function(self) { return self.D_986; }
    // implemented concepts 
    static Value_22_Concept = new Value_22_Concept(CubicBezier3D_108_Type);
    static Implements = [Value_22_Concept];
}
class QuadraticBezier2D_109_Type
{
    constructor(A_990, B_994, C_998)
    {
        // field initialization 
        this.A_990 = A_990;
        this.B_994 = B_994;
        this.C_998 = C_998;
        this.Type_1466 = QuadraticBezier2D_109_Type.Value_22_Concept.Type_1466;
        this.FieldTypes_1467 = QuadraticBezier2D_109_Type.Value_22_Concept.FieldTypes_1467;
        this.FieldNames_1468 = QuadraticBezier2D_109_Type.Value_22_Concept.FieldNames_1468;
        this.FieldValues_1470 = QuadraticBezier2D_109_Type.Value_22_Concept.FieldValues_1470;
        this.Zero_1471 = QuadraticBezier2D_109_Type.Value_22_Concept.Zero_1471;
        this.One_1472 = QuadraticBezier2D_109_Type.Value_22_Concept.One_1472;
        this.Default_1473 = QuadraticBezier2D_109_Type.Value_22_Concept.Default_1473;
        this.MinValue_1474 = QuadraticBezier2D_109_Type.Value_22_Concept.MinValue_1474;
        this.MaxValue_1475 = QuadraticBezier2D_109_Type.Value_22_Concept.MaxValue_1475;
        this.ToString_1477 = QuadraticBezier2D_109_Type.Value_22_Concept.ToString_1477;
    }
    // field accessors
    static A_990 = function(self) { return self.A_990; }
    static B_994 = function(self) { return self.B_994; }
    static C_998 = function(self) { return self.C_998; }
    // implemented concepts 
    static Value_22_Concept = new Value_22_Concept(QuadraticBezier2D_109_Type);
    static Implements = [Value_22_Concept];
}
class QuadraticBezier3D_110_Type
{
    constructor(A_1002, B_1006, C_1010)
    {
        // field initialization 
        this.A_1002 = A_1002;
        this.B_1006 = B_1006;
        this.C_1010 = C_1010;
        this.Type_1466 = QuadraticBezier3D_110_Type.Value_22_Concept.Type_1466;
        this.FieldTypes_1467 = QuadraticBezier3D_110_Type.Value_22_Concept.FieldTypes_1467;
        this.FieldNames_1468 = QuadraticBezier3D_110_Type.Value_22_Concept.FieldNames_1468;
        this.FieldValues_1470 = QuadraticBezier3D_110_Type.Value_22_Concept.FieldValues_1470;
        this.Zero_1471 = QuadraticBezier3D_110_Type.Value_22_Concept.Zero_1471;
        this.One_1472 = QuadraticBezier3D_110_Type.Value_22_Concept.One_1472;
        this.Default_1473 = QuadraticBezier3D_110_Type.Value_22_Concept.Default_1473;
        this.MinValue_1474 = QuadraticBezier3D_110_Type.Value_22_Concept.MinValue_1474;
        this.MaxValue_1475 = QuadraticBezier3D_110_Type.Value_22_Concept.MaxValue_1475;
        this.ToString_1477 = QuadraticBezier3D_110_Type.Value_22_Concept.ToString_1477;
    }
    // field accessors
    static A_1002 = function(self) { return self.A_1002; }
    static B_1006 = function(self) { return self.B_1006; }
    static C_1010 = function(self) { return self.C_1010; }
    // implemented concepts 
    static Value_22_Concept = new Value_22_Concept(QuadraticBezier3D_110_Type);
    static Implements = [Value_22_Concept];
}
class Area_111_Type
{
    constructor(MetersSquared_1014)
    {
        // field initialization 
        this.MetersSquared_1014 = MetersSquared_1014;
        this.Type_1466 = Area_111_Type.Value_22_Concept.Type_1466;
        this.FieldTypes_1467 = Area_111_Type.Value_22_Concept.FieldTypes_1467;
        this.FieldNames_1468 = Area_111_Type.Value_22_Concept.FieldNames_1468;
        this.FieldValues_1470 = Area_111_Type.Value_22_Concept.FieldValues_1470;
        this.Zero_1471 = Area_111_Type.Value_22_Concept.Zero_1471;
        this.One_1472 = Area_111_Type.Value_22_Concept.One_1472;
        this.Default_1473 = Area_111_Type.Value_22_Concept.Default_1473;
        this.MinValue_1474 = Area_111_Type.Value_22_Concept.MinValue_1474;
        this.MaxValue_1475 = Area_111_Type.Value_22_Concept.MaxValue_1475;
        this.ToString_1477 = Area_111_Type.Value_22_Concept.ToString_1477;
        this.Add_1445 = Area_111_Type.ScalarArithmetic_20_Concept.Add_1445;
        this.Subtract_1448 = Area_111_Type.ScalarArithmetic_20_Concept.Subtract_1448;
        this.Multiply_1451 = Area_111_Type.ScalarArithmetic_20_Concept.Multiply_1451;
        this.Divide_1454 = Area_111_Type.ScalarArithmetic_20_Concept.Divide_1454;
        this.Modulo_1457 = Area_111_Type.ScalarArithmetic_20_Concept.Modulo_1457;
        this.Equals_1425 = Area_111_Type.Equatable_18_Concept.Equals_1425;
        this.Compare_1422 = Area_111_Type.Comparable_17_Concept.Compare_1422;
        this.Magnitude_1419 = Area_111_Type.Magnitude_16_Concept.Magnitude_1419;
        this.Value_1417 = Area_111_Type.Measure_14_Concept.Value_1417;
    }
    // field accessors
    static MetersSquared_1014 = function(self) { return self.MetersSquared_1014; }
    // implemented concepts 
    static Value_22_Concept = new Value_22_Concept(Area_111_Type);
    static ScalarArithmetic_20_Concept = new ScalarArithmetic_20_Concept(Area_111_Type);
    static Equatable_18_Concept = new Equatable_18_Concept(Area_111_Type);
    static Comparable_17_Concept = new Comparable_17_Concept(Area_111_Type);
    static Magnitude_16_Concept = new Magnitude_16_Concept(Area_111_Type);
    static Measure_14_Concept = new Measure_14_Concept(Area_111_Type);
    static Implements = [Value_22_Concept,ScalarArithmetic_20_Concept,Equatable_18_Concept,Comparable_17_Concept,Magnitude_16_Concept,Measure_14_Concept];
}
class Volume_112_Type
{
    constructor(MetersCubed_1018)
    {
        // field initialization 
        this.MetersCubed_1018 = MetersCubed_1018;
        this.Type_1466 = Volume_112_Type.Value_22_Concept.Type_1466;
        this.FieldTypes_1467 = Volume_112_Type.Value_22_Concept.FieldTypes_1467;
        this.FieldNames_1468 = Volume_112_Type.Value_22_Concept.FieldNames_1468;
        this.FieldValues_1470 = Volume_112_Type.Value_22_Concept.FieldValues_1470;
        this.Zero_1471 = Volume_112_Type.Value_22_Concept.Zero_1471;
        this.One_1472 = Volume_112_Type.Value_22_Concept.One_1472;
        this.Default_1473 = Volume_112_Type.Value_22_Concept.Default_1473;
        this.MinValue_1474 = Volume_112_Type.Value_22_Concept.MinValue_1474;
        this.MaxValue_1475 = Volume_112_Type.Value_22_Concept.MaxValue_1475;
        this.ToString_1477 = Volume_112_Type.Value_22_Concept.ToString_1477;
        this.Add_1445 = Volume_112_Type.ScalarArithmetic_20_Concept.Add_1445;
        this.Subtract_1448 = Volume_112_Type.ScalarArithmetic_20_Concept.Subtract_1448;
        this.Multiply_1451 = Volume_112_Type.ScalarArithmetic_20_Concept.Multiply_1451;
        this.Divide_1454 = Volume_112_Type.ScalarArithmetic_20_Concept.Divide_1454;
        this.Modulo_1457 = Volume_112_Type.ScalarArithmetic_20_Concept.Modulo_1457;
        this.Equals_1425 = Volume_112_Type.Equatable_18_Concept.Equals_1425;
        this.Compare_1422 = Volume_112_Type.Comparable_17_Concept.Compare_1422;
        this.Magnitude_1419 = Volume_112_Type.Magnitude_16_Concept.Magnitude_1419;
        this.Value_1417 = Volume_112_Type.Measure_14_Concept.Value_1417;
    }
    // field accessors
    static MetersCubed_1018 = function(self) { return self.MetersCubed_1018; }
    // implemented concepts 
    static Value_22_Concept = new Value_22_Concept(Volume_112_Type);
    static ScalarArithmetic_20_Concept = new ScalarArithmetic_20_Concept(Volume_112_Type);
    static Equatable_18_Concept = new Equatable_18_Concept(Volume_112_Type);
    static Comparable_17_Concept = new Comparable_17_Concept(Volume_112_Type);
    static Magnitude_16_Concept = new Magnitude_16_Concept(Volume_112_Type);
    static Measure_14_Concept = new Measure_14_Concept(Volume_112_Type);
    static Implements = [Value_22_Concept,ScalarArithmetic_20_Concept,Equatable_18_Concept,Comparable_17_Concept,Magnitude_16_Concept,Measure_14_Concept];
}
class Velocity_113_Type
{
    constructor(MetersPerSecond_1022)
    {
        // field initialization 
        this.MetersPerSecond_1022 = MetersPerSecond_1022;
        this.Type_1466 = Velocity_113_Type.Value_22_Concept.Type_1466;
        this.FieldTypes_1467 = Velocity_113_Type.Value_22_Concept.FieldTypes_1467;
        this.FieldNames_1468 = Velocity_113_Type.Value_22_Concept.FieldNames_1468;
        this.FieldValues_1470 = Velocity_113_Type.Value_22_Concept.FieldValues_1470;
        this.Zero_1471 = Velocity_113_Type.Value_22_Concept.Zero_1471;
        this.One_1472 = Velocity_113_Type.Value_22_Concept.One_1472;
        this.Default_1473 = Velocity_113_Type.Value_22_Concept.Default_1473;
        this.MinValue_1474 = Velocity_113_Type.Value_22_Concept.MinValue_1474;
        this.MaxValue_1475 = Velocity_113_Type.Value_22_Concept.MaxValue_1475;
        this.ToString_1477 = Velocity_113_Type.Value_22_Concept.ToString_1477;
        this.Add_1445 = Velocity_113_Type.ScalarArithmetic_20_Concept.Add_1445;
        this.Subtract_1448 = Velocity_113_Type.ScalarArithmetic_20_Concept.Subtract_1448;
        this.Multiply_1451 = Velocity_113_Type.ScalarArithmetic_20_Concept.Multiply_1451;
        this.Divide_1454 = Velocity_113_Type.ScalarArithmetic_20_Concept.Divide_1454;
        this.Modulo_1457 = Velocity_113_Type.ScalarArithmetic_20_Concept.Modulo_1457;
        this.Equals_1425 = Velocity_113_Type.Equatable_18_Concept.Equals_1425;
        this.Compare_1422 = Velocity_113_Type.Comparable_17_Concept.Compare_1422;
        this.Magnitude_1419 = Velocity_113_Type.Magnitude_16_Concept.Magnitude_1419;
        this.Value_1417 = Velocity_113_Type.Measure_14_Concept.Value_1417;
    }
    // field accessors
    static MetersPerSecond_1022 = function(self) { return self.MetersPerSecond_1022; }
    // implemented concepts 
    static Value_22_Concept = new Value_22_Concept(Velocity_113_Type);
    static ScalarArithmetic_20_Concept = new ScalarArithmetic_20_Concept(Velocity_113_Type);
    static Equatable_18_Concept = new Equatable_18_Concept(Velocity_113_Type);
    static Comparable_17_Concept = new Comparable_17_Concept(Velocity_113_Type);
    static Magnitude_16_Concept = new Magnitude_16_Concept(Velocity_113_Type);
    static Measure_14_Concept = new Measure_14_Concept(Velocity_113_Type);
    static Implements = [Value_22_Concept,ScalarArithmetic_20_Concept,Equatable_18_Concept,Comparable_17_Concept,Magnitude_16_Concept,Measure_14_Concept];
}
class Acceleration_114_Type
{
    constructor(MetersPerSecondSquared_1026)
    {
        // field initialization 
        this.MetersPerSecondSquared_1026 = MetersPerSecondSquared_1026;
        this.Type_1466 = Acceleration_114_Type.Value_22_Concept.Type_1466;
        this.FieldTypes_1467 = Acceleration_114_Type.Value_22_Concept.FieldTypes_1467;
        this.FieldNames_1468 = Acceleration_114_Type.Value_22_Concept.FieldNames_1468;
        this.FieldValues_1470 = Acceleration_114_Type.Value_22_Concept.FieldValues_1470;
        this.Zero_1471 = Acceleration_114_Type.Value_22_Concept.Zero_1471;
        this.One_1472 = Acceleration_114_Type.Value_22_Concept.One_1472;
        this.Default_1473 = Acceleration_114_Type.Value_22_Concept.Default_1473;
        this.MinValue_1474 = Acceleration_114_Type.Value_22_Concept.MinValue_1474;
        this.MaxValue_1475 = Acceleration_114_Type.Value_22_Concept.MaxValue_1475;
        this.ToString_1477 = Acceleration_114_Type.Value_22_Concept.ToString_1477;
        this.Add_1445 = Acceleration_114_Type.ScalarArithmetic_20_Concept.Add_1445;
        this.Subtract_1448 = Acceleration_114_Type.ScalarArithmetic_20_Concept.Subtract_1448;
        this.Multiply_1451 = Acceleration_114_Type.ScalarArithmetic_20_Concept.Multiply_1451;
        this.Divide_1454 = Acceleration_114_Type.ScalarArithmetic_20_Concept.Divide_1454;
        this.Modulo_1457 = Acceleration_114_Type.ScalarArithmetic_20_Concept.Modulo_1457;
        this.Equals_1425 = Acceleration_114_Type.Equatable_18_Concept.Equals_1425;
        this.Compare_1422 = Acceleration_114_Type.Comparable_17_Concept.Compare_1422;
        this.Magnitude_1419 = Acceleration_114_Type.Magnitude_16_Concept.Magnitude_1419;
        this.Value_1417 = Acceleration_114_Type.Measure_14_Concept.Value_1417;
    }
    // field accessors
    static MetersPerSecondSquared_1026 = function(self) { return self.MetersPerSecondSquared_1026; }
    // implemented concepts 
    static Value_22_Concept = new Value_22_Concept(Acceleration_114_Type);
    static ScalarArithmetic_20_Concept = new ScalarArithmetic_20_Concept(Acceleration_114_Type);
    static Equatable_18_Concept = new Equatable_18_Concept(Acceleration_114_Type);
    static Comparable_17_Concept = new Comparable_17_Concept(Acceleration_114_Type);
    static Magnitude_16_Concept = new Magnitude_16_Concept(Acceleration_114_Type);
    static Measure_14_Concept = new Measure_14_Concept(Acceleration_114_Type);
    static Implements = [Value_22_Concept,ScalarArithmetic_20_Concept,Equatable_18_Concept,Comparable_17_Concept,Magnitude_16_Concept,Measure_14_Concept];
}
class Force_115_Type
{
    constructor(Newtons_1030)
    {
        // field initialization 
        this.Newtons_1030 = Newtons_1030;
        this.Type_1466 = Force_115_Type.Value_22_Concept.Type_1466;
        this.FieldTypes_1467 = Force_115_Type.Value_22_Concept.FieldTypes_1467;
        this.FieldNames_1468 = Force_115_Type.Value_22_Concept.FieldNames_1468;
        this.FieldValues_1470 = Force_115_Type.Value_22_Concept.FieldValues_1470;
        this.Zero_1471 = Force_115_Type.Value_22_Concept.Zero_1471;
        this.One_1472 = Force_115_Type.Value_22_Concept.One_1472;
        this.Default_1473 = Force_115_Type.Value_22_Concept.Default_1473;
        this.MinValue_1474 = Force_115_Type.Value_22_Concept.MinValue_1474;
        this.MaxValue_1475 = Force_115_Type.Value_22_Concept.MaxValue_1475;
        this.ToString_1477 = Force_115_Type.Value_22_Concept.ToString_1477;
        this.Add_1445 = Force_115_Type.ScalarArithmetic_20_Concept.Add_1445;
        this.Subtract_1448 = Force_115_Type.ScalarArithmetic_20_Concept.Subtract_1448;
        this.Multiply_1451 = Force_115_Type.ScalarArithmetic_20_Concept.Multiply_1451;
        this.Divide_1454 = Force_115_Type.ScalarArithmetic_20_Concept.Divide_1454;
        this.Modulo_1457 = Force_115_Type.ScalarArithmetic_20_Concept.Modulo_1457;
        this.Equals_1425 = Force_115_Type.Equatable_18_Concept.Equals_1425;
        this.Compare_1422 = Force_115_Type.Comparable_17_Concept.Compare_1422;
        this.Magnitude_1419 = Force_115_Type.Magnitude_16_Concept.Magnitude_1419;
        this.Value_1417 = Force_115_Type.Measure_14_Concept.Value_1417;
    }
    // field accessors
    static Newtons_1030 = function(self) { return self.Newtons_1030; }
    // implemented concepts 
    static Value_22_Concept = new Value_22_Concept(Force_115_Type);
    static ScalarArithmetic_20_Concept = new ScalarArithmetic_20_Concept(Force_115_Type);
    static Equatable_18_Concept = new Equatable_18_Concept(Force_115_Type);
    static Comparable_17_Concept = new Comparable_17_Concept(Force_115_Type);
    static Magnitude_16_Concept = new Magnitude_16_Concept(Force_115_Type);
    static Measure_14_Concept = new Measure_14_Concept(Force_115_Type);
    static Implements = [Value_22_Concept,ScalarArithmetic_20_Concept,Equatable_18_Concept,Comparable_17_Concept,Magnitude_16_Concept,Measure_14_Concept];
}
class Pressure_116_Type
{
    constructor(Pascals_1034)
    {
        // field initialization 
        this.Pascals_1034 = Pascals_1034;
        this.Type_1466 = Pressure_116_Type.Value_22_Concept.Type_1466;
        this.FieldTypes_1467 = Pressure_116_Type.Value_22_Concept.FieldTypes_1467;
        this.FieldNames_1468 = Pressure_116_Type.Value_22_Concept.FieldNames_1468;
        this.FieldValues_1470 = Pressure_116_Type.Value_22_Concept.FieldValues_1470;
        this.Zero_1471 = Pressure_116_Type.Value_22_Concept.Zero_1471;
        this.One_1472 = Pressure_116_Type.Value_22_Concept.One_1472;
        this.Default_1473 = Pressure_116_Type.Value_22_Concept.Default_1473;
        this.MinValue_1474 = Pressure_116_Type.Value_22_Concept.MinValue_1474;
        this.MaxValue_1475 = Pressure_116_Type.Value_22_Concept.MaxValue_1475;
        this.ToString_1477 = Pressure_116_Type.Value_22_Concept.ToString_1477;
        this.Add_1445 = Pressure_116_Type.ScalarArithmetic_20_Concept.Add_1445;
        this.Subtract_1448 = Pressure_116_Type.ScalarArithmetic_20_Concept.Subtract_1448;
        this.Multiply_1451 = Pressure_116_Type.ScalarArithmetic_20_Concept.Multiply_1451;
        this.Divide_1454 = Pressure_116_Type.ScalarArithmetic_20_Concept.Divide_1454;
        this.Modulo_1457 = Pressure_116_Type.ScalarArithmetic_20_Concept.Modulo_1457;
        this.Equals_1425 = Pressure_116_Type.Equatable_18_Concept.Equals_1425;
        this.Compare_1422 = Pressure_116_Type.Comparable_17_Concept.Compare_1422;
        this.Magnitude_1419 = Pressure_116_Type.Magnitude_16_Concept.Magnitude_1419;
        this.Value_1417 = Pressure_116_Type.Measure_14_Concept.Value_1417;
    }
    // field accessors
    static Pascals_1034 = function(self) { return self.Pascals_1034; }
    // implemented concepts 
    static Value_22_Concept = new Value_22_Concept(Pressure_116_Type);
    static ScalarArithmetic_20_Concept = new ScalarArithmetic_20_Concept(Pressure_116_Type);
    static Equatable_18_Concept = new Equatable_18_Concept(Pressure_116_Type);
    static Comparable_17_Concept = new Comparable_17_Concept(Pressure_116_Type);
    static Magnitude_16_Concept = new Magnitude_16_Concept(Pressure_116_Type);
    static Measure_14_Concept = new Measure_14_Concept(Pressure_116_Type);
    static Implements = [Value_22_Concept,ScalarArithmetic_20_Concept,Equatable_18_Concept,Comparable_17_Concept,Magnitude_16_Concept,Measure_14_Concept];
}
class Energy_117_Type
{
    constructor(Joules_1038)
    {
        // field initialization 
        this.Joules_1038 = Joules_1038;
        this.Type_1466 = Energy_117_Type.Value_22_Concept.Type_1466;
        this.FieldTypes_1467 = Energy_117_Type.Value_22_Concept.FieldTypes_1467;
        this.FieldNames_1468 = Energy_117_Type.Value_22_Concept.FieldNames_1468;
        this.FieldValues_1470 = Energy_117_Type.Value_22_Concept.FieldValues_1470;
        this.Zero_1471 = Energy_117_Type.Value_22_Concept.Zero_1471;
        this.One_1472 = Energy_117_Type.Value_22_Concept.One_1472;
        this.Default_1473 = Energy_117_Type.Value_22_Concept.Default_1473;
        this.MinValue_1474 = Energy_117_Type.Value_22_Concept.MinValue_1474;
        this.MaxValue_1475 = Energy_117_Type.Value_22_Concept.MaxValue_1475;
        this.ToString_1477 = Energy_117_Type.Value_22_Concept.ToString_1477;
        this.Add_1445 = Energy_117_Type.ScalarArithmetic_20_Concept.Add_1445;
        this.Subtract_1448 = Energy_117_Type.ScalarArithmetic_20_Concept.Subtract_1448;
        this.Multiply_1451 = Energy_117_Type.ScalarArithmetic_20_Concept.Multiply_1451;
        this.Divide_1454 = Energy_117_Type.ScalarArithmetic_20_Concept.Divide_1454;
        this.Modulo_1457 = Energy_117_Type.ScalarArithmetic_20_Concept.Modulo_1457;
        this.Equals_1425 = Energy_117_Type.Equatable_18_Concept.Equals_1425;
        this.Compare_1422 = Energy_117_Type.Comparable_17_Concept.Compare_1422;
        this.Magnitude_1419 = Energy_117_Type.Magnitude_16_Concept.Magnitude_1419;
        this.Value_1417 = Energy_117_Type.Measure_14_Concept.Value_1417;
    }
    // field accessors
    static Joules_1038 = function(self) { return self.Joules_1038; }
    // implemented concepts 
    static Value_22_Concept = new Value_22_Concept(Energy_117_Type);
    static ScalarArithmetic_20_Concept = new ScalarArithmetic_20_Concept(Energy_117_Type);
    static Equatable_18_Concept = new Equatable_18_Concept(Energy_117_Type);
    static Comparable_17_Concept = new Comparable_17_Concept(Energy_117_Type);
    static Magnitude_16_Concept = new Magnitude_16_Concept(Energy_117_Type);
    static Measure_14_Concept = new Measure_14_Concept(Energy_117_Type);
    static Implements = [Value_22_Concept,ScalarArithmetic_20_Concept,Equatable_18_Concept,Comparable_17_Concept,Magnitude_16_Concept,Measure_14_Concept];
}
class Memory_118_Type
{
    constructor(Bytes_1042)
    {
        // field initialization 
        this.Bytes_1042 = Bytes_1042;
        this.Type_1466 = Memory_118_Type.Value_22_Concept.Type_1466;
        this.FieldTypes_1467 = Memory_118_Type.Value_22_Concept.FieldTypes_1467;
        this.FieldNames_1468 = Memory_118_Type.Value_22_Concept.FieldNames_1468;
        this.FieldValues_1470 = Memory_118_Type.Value_22_Concept.FieldValues_1470;
        this.Zero_1471 = Memory_118_Type.Value_22_Concept.Zero_1471;
        this.One_1472 = Memory_118_Type.Value_22_Concept.One_1472;
        this.Default_1473 = Memory_118_Type.Value_22_Concept.Default_1473;
        this.MinValue_1474 = Memory_118_Type.Value_22_Concept.MinValue_1474;
        this.MaxValue_1475 = Memory_118_Type.Value_22_Concept.MaxValue_1475;
        this.ToString_1477 = Memory_118_Type.Value_22_Concept.ToString_1477;
        this.Add_1445 = Memory_118_Type.ScalarArithmetic_20_Concept.Add_1445;
        this.Subtract_1448 = Memory_118_Type.ScalarArithmetic_20_Concept.Subtract_1448;
        this.Multiply_1451 = Memory_118_Type.ScalarArithmetic_20_Concept.Multiply_1451;
        this.Divide_1454 = Memory_118_Type.ScalarArithmetic_20_Concept.Divide_1454;
        this.Modulo_1457 = Memory_118_Type.ScalarArithmetic_20_Concept.Modulo_1457;
        this.Equals_1425 = Memory_118_Type.Equatable_18_Concept.Equals_1425;
        this.Compare_1422 = Memory_118_Type.Comparable_17_Concept.Compare_1422;
        this.Magnitude_1419 = Memory_118_Type.Magnitude_16_Concept.Magnitude_1419;
        this.Value_1417 = Memory_118_Type.Measure_14_Concept.Value_1417;
    }
    // field accessors
    static Bytes_1042 = function(self) { return self.Bytes_1042; }
    // implemented concepts 
    static Value_22_Concept = new Value_22_Concept(Memory_118_Type);
    static ScalarArithmetic_20_Concept = new ScalarArithmetic_20_Concept(Memory_118_Type);
    static Equatable_18_Concept = new Equatable_18_Concept(Memory_118_Type);
    static Comparable_17_Concept = new Comparable_17_Concept(Memory_118_Type);
    static Magnitude_16_Concept = new Magnitude_16_Concept(Memory_118_Type);
    static Measure_14_Concept = new Measure_14_Concept(Memory_118_Type);
    static Implements = [Value_22_Concept,ScalarArithmetic_20_Concept,Equatable_18_Concept,Comparable_17_Concept,Magnitude_16_Concept,Measure_14_Concept];
}
class Frequency_119_Type
{
    constructor(Hertz_1046)
    {
        // field initialization 
        this.Hertz_1046 = Hertz_1046;
        this.Type_1466 = Frequency_119_Type.Value_22_Concept.Type_1466;
        this.FieldTypes_1467 = Frequency_119_Type.Value_22_Concept.FieldTypes_1467;
        this.FieldNames_1468 = Frequency_119_Type.Value_22_Concept.FieldNames_1468;
        this.FieldValues_1470 = Frequency_119_Type.Value_22_Concept.FieldValues_1470;
        this.Zero_1471 = Frequency_119_Type.Value_22_Concept.Zero_1471;
        this.One_1472 = Frequency_119_Type.Value_22_Concept.One_1472;
        this.Default_1473 = Frequency_119_Type.Value_22_Concept.Default_1473;
        this.MinValue_1474 = Frequency_119_Type.Value_22_Concept.MinValue_1474;
        this.MaxValue_1475 = Frequency_119_Type.Value_22_Concept.MaxValue_1475;
        this.ToString_1477 = Frequency_119_Type.Value_22_Concept.ToString_1477;
        this.Add_1445 = Frequency_119_Type.ScalarArithmetic_20_Concept.Add_1445;
        this.Subtract_1448 = Frequency_119_Type.ScalarArithmetic_20_Concept.Subtract_1448;
        this.Multiply_1451 = Frequency_119_Type.ScalarArithmetic_20_Concept.Multiply_1451;
        this.Divide_1454 = Frequency_119_Type.ScalarArithmetic_20_Concept.Divide_1454;
        this.Modulo_1457 = Frequency_119_Type.ScalarArithmetic_20_Concept.Modulo_1457;
        this.Equals_1425 = Frequency_119_Type.Equatable_18_Concept.Equals_1425;
        this.Compare_1422 = Frequency_119_Type.Comparable_17_Concept.Compare_1422;
        this.Magnitude_1419 = Frequency_119_Type.Magnitude_16_Concept.Magnitude_1419;
        this.Value_1417 = Frequency_119_Type.Measure_14_Concept.Value_1417;
    }
    // field accessors
    static Hertz_1046 = function(self) { return self.Hertz_1046; }
    // implemented concepts 
    static Value_22_Concept = new Value_22_Concept(Frequency_119_Type);
    static ScalarArithmetic_20_Concept = new ScalarArithmetic_20_Concept(Frequency_119_Type);
    static Equatable_18_Concept = new Equatable_18_Concept(Frequency_119_Type);
    static Comparable_17_Concept = new Comparable_17_Concept(Frequency_119_Type);
    static Magnitude_16_Concept = new Magnitude_16_Concept(Frequency_119_Type);
    static Measure_14_Concept = new Measure_14_Concept(Frequency_119_Type);
    static Implements = [Value_22_Concept,ScalarArithmetic_20_Concept,Equatable_18_Concept,Comparable_17_Concept,Magnitude_16_Concept,Measure_14_Concept];
}
class Loudness_120_Type
{
    constructor(Decibels_1050)
    {
        // field initialization 
        this.Decibels_1050 = Decibels_1050;
        this.Type_1466 = Loudness_120_Type.Value_22_Concept.Type_1466;
        this.FieldTypes_1467 = Loudness_120_Type.Value_22_Concept.FieldTypes_1467;
        this.FieldNames_1468 = Loudness_120_Type.Value_22_Concept.FieldNames_1468;
        this.FieldValues_1470 = Loudness_120_Type.Value_22_Concept.FieldValues_1470;
        this.Zero_1471 = Loudness_120_Type.Value_22_Concept.Zero_1471;
        this.One_1472 = Loudness_120_Type.Value_22_Concept.One_1472;
        this.Default_1473 = Loudness_120_Type.Value_22_Concept.Default_1473;
        this.MinValue_1474 = Loudness_120_Type.Value_22_Concept.MinValue_1474;
        this.MaxValue_1475 = Loudness_120_Type.Value_22_Concept.MaxValue_1475;
        this.ToString_1477 = Loudness_120_Type.Value_22_Concept.ToString_1477;
        this.Add_1445 = Loudness_120_Type.ScalarArithmetic_20_Concept.Add_1445;
        this.Subtract_1448 = Loudness_120_Type.ScalarArithmetic_20_Concept.Subtract_1448;
        this.Multiply_1451 = Loudness_120_Type.ScalarArithmetic_20_Concept.Multiply_1451;
        this.Divide_1454 = Loudness_120_Type.ScalarArithmetic_20_Concept.Divide_1454;
        this.Modulo_1457 = Loudness_120_Type.ScalarArithmetic_20_Concept.Modulo_1457;
        this.Equals_1425 = Loudness_120_Type.Equatable_18_Concept.Equals_1425;
        this.Compare_1422 = Loudness_120_Type.Comparable_17_Concept.Compare_1422;
        this.Magnitude_1419 = Loudness_120_Type.Magnitude_16_Concept.Magnitude_1419;
        this.Value_1417 = Loudness_120_Type.Measure_14_Concept.Value_1417;
    }
    // field accessors
    static Decibels_1050 = function(self) { return self.Decibels_1050; }
    // implemented concepts 
    static Value_22_Concept = new Value_22_Concept(Loudness_120_Type);
    static ScalarArithmetic_20_Concept = new ScalarArithmetic_20_Concept(Loudness_120_Type);
    static Equatable_18_Concept = new Equatable_18_Concept(Loudness_120_Type);
    static Comparable_17_Concept = new Comparable_17_Concept(Loudness_120_Type);
    static Magnitude_16_Concept = new Magnitude_16_Concept(Loudness_120_Type);
    static Measure_14_Concept = new Measure_14_Concept(Loudness_120_Type);
    static Implements = [Value_22_Concept,ScalarArithmetic_20_Concept,Equatable_18_Concept,Comparable_17_Concept,Magnitude_16_Concept,Measure_14_Concept];
}
class LuminousIntensity_121_Type
{
    constructor(Candelas_1054)
    {
        // field initialization 
        this.Candelas_1054 = Candelas_1054;
        this.Type_1466 = LuminousIntensity_121_Type.Value_22_Concept.Type_1466;
        this.FieldTypes_1467 = LuminousIntensity_121_Type.Value_22_Concept.FieldTypes_1467;
        this.FieldNames_1468 = LuminousIntensity_121_Type.Value_22_Concept.FieldNames_1468;
        this.FieldValues_1470 = LuminousIntensity_121_Type.Value_22_Concept.FieldValues_1470;
        this.Zero_1471 = LuminousIntensity_121_Type.Value_22_Concept.Zero_1471;
        this.One_1472 = LuminousIntensity_121_Type.Value_22_Concept.One_1472;
        this.Default_1473 = LuminousIntensity_121_Type.Value_22_Concept.Default_1473;
        this.MinValue_1474 = LuminousIntensity_121_Type.Value_22_Concept.MinValue_1474;
        this.MaxValue_1475 = LuminousIntensity_121_Type.Value_22_Concept.MaxValue_1475;
        this.ToString_1477 = LuminousIntensity_121_Type.Value_22_Concept.ToString_1477;
        this.Add_1445 = LuminousIntensity_121_Type.ScalarArithmetic_20_Concept.Add_1445;
        this.Subtract_1448 = LuminousIntensity_121_Type.ScalarArithmetic_20_Concept.Subtract_1448;
        this.Multiply_1451 = LuminousIntensity_121_Type.ScalarArithmetic_20_Concept.Multiply_1451;
        this.Divide_1454 = LuminousIntensity_121_Type.ScalarArithmetic_20_Concept.Divide_1454;
        this.Modulo_1457 = LuminousIntensity_121_Type.ScalarArithmetic_20_Concept.Modulo_1457;
        this.Equals_1425 = LuminousIntensity_121_Type.Equatable_18_Concept.Equals_1425;
        this.Compare_1422 = LuminousIntensity_121_Type.Comparable_17_Concept.Compare_1422;
        this.Magnitude_1419 = LuminousIntensity_121_Type.Magnitude_16_Concept.Magnitude_1419;
        this.Value_1417 = LuminousIntensity_121_Type.Measure_14_Concept.Value_1417;
    }
    // field accessors
    static Candelas_1054 = function(self) { return self.Candelas_1054; }
    // implemented concepts 
    static Value_22_Concept = new Value_22_Concept(LuminousIntensity_121_Type);
    static ScalarArithmetic_20_Concept = new ScalarArithmetic_20_Concept(LuminousIntensity_121_Type);
    static Equatable_18_Concept = new Equatable_18_Concept(LuminousIntensity_121_Type);
    static Comparable_17_Concept = new Comparable_17_Concept(LuminousIntensity_121_Type);
    static Magnitude_16_Concept = new Magnitude_16_Concept(LuminousIntensity_121_Type);
    static Measure_14_Concept = new Measure_14_Concept(LuminousIntensity_121_Type);
    static Implements = [Value_22_Concept,ScalarArithmetic_20_Concept,Equatable_18_Concept,Comparable_17_Concept,Magnitude_16_Concept,Measure_14_Concept];
}
class ElectricPotential_122_Type
{
    constructor(Volts_1058)
    {
        // field initialization 
        this.Volts_1058 = Volts_1058;
        this.Type_1466 = ElectricPotential_122_Type.Value_22_Concept.Type_1466;
        this.FieldTypes_1467 = ElectricPotential_122_Type.Value_22_Concept.FieldTypes_1467;
        this.FieldNames_1468 = ElectricPotential_122_Type.Value_22_Concept.FieldNames_1468;
        this.FieldValues_1470 = ElectricPotential_122_Type.Value_22_Concept.FieldValues_1470;
        this.Zero_1471 = ElectricPotential_122_Type.Value_22_Concept.Zero_1471;
        this.One_1472 = ElectricPotential_122_Type.Value_22_Concept.One_1472;
        this.Default_1473 = ElectricPotential_122_Type.Value_22_Concept.Default_1473;
        this.MinValue_1474 = ElectricPotential_122_Type.Value_22_Concept.MinValue_1474;
        this.MaxValue_1475 = ElectricPotential_122_Type.Value_22_Concept.MaxValue_1475;
        this.ToString_1477 = ElectricPotential_122_Type.Value_22_Concept.ToString_1477;
        this.Add_1445 = ElectricPotential_122_Type.ScalarArithmetic_20_Concept.Add_1445;
        this.Subtract_1448 = ElectricPotential_122_Type.ScalarArithmetic_20_Concept.Subtract_1448;
        this.Multiply_1451 = ElectricPotential_122_Type.ScalarArithmetic_20_Concept.Multiply_1451;
        this.Divide_1454 = ElectricPotential_122_Type.ScalarArithmetic_20_Concept.Divide_1454;
        this.Modulo_1457 = ElectricPotential_122_Type.ScalarArithmetic_20_Concept.Modulo_1457;
        this.Equals_1425 = ElectricPotential_122_Type.Equatable_18_Concept.Equals_1425;
        this.Compare_1422 = ElectricPotential_122_Type.Comparable_17_Concept.Compare_1422;
        this.Magnitude_1419 = ElectricPotential_122_Type.Magnitude_16_Concept.Magnitude_1419;
        this.Value_1417 = ElectricPotential_122_Type.Measure_14_Concept.Value_1417;
    }
    // field accessors
    static Volts_1058 = function(self) { return self.Volts_1058; }
    // implemented concepts 
    static Value_22_Concept = new Value_22_Concept(ElectricPotential_122_Type);
    static ScalarArithmetic_20_Concept = new ScalarArithmetic_20_Concept(ElectricPotential_122_Type);
    static Equatable_18_Concept = new Equatable_18_Concept(ElectricPotential_122_Type);
    static Comparable_17_Concept = new Comparable_17_Concept(ElectricPotential_122_Type);
    static Magnitude_16_Concept = new Magnitude_16_Concept(ElectricPotential_122_Type);
    static Measure_14_Concept = new Measure_14_Concept(ElectricPotential_122_Type);
    static Implements = [Value_22_Concept,ScalarArithmetic_20_Concept,Equatable_18_Concept,Comparable_17_Concept,Magnitude_16_Concept,Measure_14_Concept];
}
class ElectricCharge_123_Type
{
    constructor(Columbs_1062)
    {
        // field initialization 
        this.Columbs_1062 = Columbs_1062;
        this.Type_1466 = ElectricCharge_123_Type.Value_22_Concept.Type_1466;
        this.FieldTypes_1467 = ElectricCharge_123_Type.Value_22_Concept.FieldTypes_1467;
        this.FieldNames_1468 = ElectricCharge_123_Type.Value_22_Concept.FieldNames_1468;
        this.FieldValues_1470 = ElectricCharge_123_Type.Value_22_Concept.FieldValues_1470;
        this.Zero_1471 = ElectricCharge_123_Type.Value_22_Concept.Zero_1471;
        this.One_1472 = ElectricCharge_123_Type.Value_22_Concept.One_1472;
        this.Default_1473 = ElectricCharge_123_Type.Value_22_Concept.Default_1473;
        this.MinValue_1474 = ElectricCharge_123_Type.Value_22_Concept.MinValue_1474;
        this.MaxValue_1475 = ElectricCharge_123_Type.Value_22_Concept.MaxValue_1475;
        this.ToString_1477 = ElectricCharge_123_Type.Value_22_Concept.ToString_1477;
        this.Add_1445 = ElectricCharge_123_Type.ScalarArithmetic_20_Concept.Add_1445;
        this.Subtract_1448 = ElectricCharge_123_Type.ScalarArithmetic_20_Concept.Subtract_1448;
        this.Multiply_1451 = ElectricCharge_123_Type.ScalarArithmetic_20_Concept.Multiply_1451;
        this.Divide_1454 = ElectricCharge_123_Type.ScalarArithmetic_20_Concept.Divide_1454;
        this.Modulo_1457 = ElectricCharge_123_Type.ScalarArithmetic_20_Concept.Modulo_1457;
        this.Equals_1425 = ElectricCharge_123_Type.Equatable_18_Concept.Equals_1425;
        this.Compare_1422 = ElectricCharge_123_Type.Comparable_17_Concept.Compare_1422;
        this.Magnitude_1419 = ElectricCharge_123_Type.Magnitude_16_Concept.Magnitude_1419;
        this.Value_1417 = ElectricCharge_123_Type.Measure_14_Concept.Value_1417;
    }
    // field accessors
    static Columbs_1062 = function(self) { return self.Columbs_1062; }
    // implemented concepts 
    static Value_22_Concept = new Value_22_Concept(ElectricCharge_123_Type);
    static ScalarArithmetic_20_Concept = new ScalarArithmetic_20_Concept(ElectricCharge_123_Type);
    static Equatable_18_Concept = new Equatable_18_Concept(ElectricCharge_123_Type);
    static Comparable_17_Concept = new Comparable_17_Concept(ElectricCharge_123_Type);
    static Magnitude_16_Concept = new Magnitude_16_Concept(ElectricCharge_123_Type);
    static Measure_14_Concept = new Measure_14_Concept(ElectricCharge_123_Type);
    static Implements = [Value_22_Concept,ScalarArithmetic_20_Concept,Equatable_18_Concept,Comparable_17_Concept,Magnitude_16_Concept,Measure_14_Concept];
}
class ElectricCurrent_124_Type
{
    constructor(Amperes_1066)
    {
        // field initialization 
        this.Amperes_1066 = Amperes_1066;
        this.Type_1466 = ElectricCurrent_124_Type.Value_22_Concept.Type_1466;
        this.FieldTypes_1467 = ElectricCurrent_124_Type.Value_22_Concept.FieldTypes_1467;
        this.FieldNames_1468 = ElectricCurrent_124_Type.Value_22_Concept.FieldNames_1468;
        this.FieldValues_1470 = ElectricCurrent_124_Type.Value_22_Concept.FieldValues_1470;
        this.Zero_1471 = ElectricCurrent_124_Type.Value_22_Concept.Zero_1471;
        this.One_1472 = ElectricCurrent_124_Type.Value_22_Concept.One_1472;
        this.Default_1473 = ElectricCurrent_124_Type.Value_22_Concept.Default_1473;
        this.MinValue_1474 = ElectricCurrent_124_Type.Value_22_Concept.MinValue_1474;
        this.MaxValue_1475 = ElectricCurrent_124_Type.Value_22_Concept.MaxValue_1475;
        this.ToString_1477 = ElectricCurrent_124_Type.Value_22_Concept.ToString_1477;
        this.Add_1445 = ElectricCurrent_124_Type.ScalarArithmetic_20_Concept.Add_1445;
        this.Subtract_1448 = ElectricCurrent_124_Type.ScalarArithmetic_20_Concept.Subtract_1448;
        this.Multiply_1451 = ElectricCurrent_124_Type.ScalarArithmetic_20_Concept.Multiply_1451;
        this.Divide_1454 = ElectricCurrent_124_Type.ScalarArithmetic_20_Concept.Divide_1454;
        this.Modulo_1457 = ElectricCurrent_124_Type.ScalarArithmetic_20_Concept.Modulo_1457;
        this.Equals_1425 = ElectricCurrent_124_Type.Equatable_18_Concept.Equals_1425;
        this.Compare_1422 = ElectricCurrent_124_Type.Comparable_17_Concept.Compare_1422;
        this.Magnitude_1419 = ElectricCurrent_124_Type.Magnitude_16_Concept.Magnitude_1419;
        this.Value_1417 = ElectricCurrent_124_Type.Measure_14_Concept.Value_1417;
    }
    // field accessors
    static Amperes_1066 = function(self) { return self.Amperes_1066; }
    // implemented concepts 
    static Value_22_Concept = new Value_22_Concept(ElectricCurrent_124_Type);
    static ScalarArithmetic_20_Concept = new ScalarArithmetic_20_Concept(ElectricCurrent_124_Type);
    static Equatable_18_Concept = new Equatable_18_Concept(ElectricCurrent_124_Type);
    static Comparable_17_Concept = new Comparable_17_Concept(ElectricCurrent_124_Type);
    static Magnitude_16_Concept = new Magnitude_16_Concept(ElectricCurrent_124_Type);
    static Measure_14_Concept = new Measure_14_Concept(ElectricCurrent_124_Type);
    static Implements = [Value_22_Concept,ScalarArithmetic_20_Concept,Equatable_18_Concept,Comparable_17_Concept,Magnitude_16_Concept,Measure_14_Concept];
}
class ElectricResistance_125_Type
{
    constructor(Ohms_1070)
    {
        // field initialization 
        this.Ohms_1070 = Ohms_1070;
        this.Type_1466 = ElectricResistance_125_Type.Value_22_Concept.Type_1466;
        this.FieldTypes_1467 = ElectricResistance_125_Type.Value_22_Concept.FieldTypes_1467;
        this.FieldNames_1468 = ElectricResistance_125_Type.Value_22_Concept.FieldNames_1468;
        this.FieldValues_1470 = ElectricResistance_125_Type.Value_22_Concept.FieldValues_1470;
        this.Zero_1471 = ElectricResistance_125_Type.Value_22_Concept.Zero_1471;
        this.One_1472 = ElectricResistance_125_Type.Value_22_Concept.One_1472;
        this.Default_1473 = ElectricResistance_125_Type.Value_22_Concept.Default_1473;
        this.MinValue_1474 = ElectricResistance_125_Type.Value_22_Concept.MinValue_1474;
        this.MaxValue_1475 = ElectricResistance_125_Type.Value_22_Concept.MaxValue_1475;
        this.ToString_1477 = ElectricResistance_125_Type.Value_22_Concept.ToString_1477;
        this.Add_1445 = ElectricResistance_125_Type.ScalarArithmetic_20_Concept.Add_1445;
        this.Subtract_1448 = ElectricResistance_125_Type.ScalarArithmetic_20_Concept.Subtract_1448;
        this.Multiply_1451 = ElectricResistance_125_Type.ScalarArithmetic_20_Concept.Multiply_1451;
        this.Divide_1454 = ElectricResistance_125_Type.ScalarArithmetic_20_Concept.Divide_1454;
        this.Modulo_1457 = ElectricResistance_125_Type.ScalarArithmetic_20_Concept.Modulo_1457;
        this.Equals_1425 = ElectricResistance_125_Type.Equatable_18_Concept.Equals_1425;
        this.Compare_1422 = ElectricResistance_125_Type.Comparable_17_Concept.Compare_1422;
        this.Magnitude_1419 = ElectricResistance_125_Type.Magnitude_16_Concept.Magnitude_1419;
        this.Value_1417 = ElectricResistance_125_Type.Measure_14_Concept.Value_1417;
    }
    // field accessors
    static Ohms_1070 = function(self) { return self.Ohms_1070; }
    // implemented concepts 
    static Value_22_Concept = new Value_22_Concept(ElectricResistance_125_Type);
    static ScalarArithmetic_20_Concept = new ScalarArithmetic_20_Concept(ElectricResistance_125_Type);
    static Equatable_18_Concept = new Equatable_18_Concept(ElectricResistance_125_Type);
    static Comparable_17_Concept = new Comparable_17_Concept(ElectricResistance_125_Type);
    static Magnitude_16_Concept = new Magnitude_16_Concept(ElectricResistance_125_Type);
    static Measure_14_Concept = new Measure_14_Concept(ElectricResistance_125_Type);
    static Implements = [Value_22_Concept,ScalarArithmetic_20_Concept,Equatable_18_Concept,Comparable_17_Concept,Magnitude_16_Concept,Measure_14_Concept];
}
class Power_126_Type
{
    constructor(Watts_1074)
    {
        // field initialization 
        this.Watts_1074 = Watts_1074;
        this.Type_1466 = Power_126_Type.Value_22_Concept.Type_1466;
        this.FieldTypes_1467 = Power_126_Type.Value_22_Concept.FieldTypes_1467;
        this.FieldNames_1468 = Power_126_Type.Value_22_Concept.FieldNames_1468;
        this.FieldValues_1470 = Power_126_Type.Value_22_Concept.FieldValues_1470;
        this.Zero_1471 = Power_126_Type.Value_22_Concept.Zero_1471;
        this.One_1472 = Power_126_Type.Value_22_Concept.One_1472;
        this.Default_1473 = Power_126_Type.Value_22_Concept.Default_1473;
        this.MinValue_1474 = Power_126_Type.Value_22_Concept.MinValue_1474;
        this.MaxValue_1475 = Power_126_Type.Value_22_Concept.MaxValue_1475;
        this.ToString_1477 = Power_126_Type.Value_22_Concept.ToString_1477;
        this.Add_1445 = Power_126_Type.ScalarArithmetic_20_Concept.Add_1445;
        this.Subtract_1448 = Power_126_Type.ScalarArithmetic_20_Concept.Subtract_1448;
        this.Multiply_1451 = Power_126_Type.ScalarArithmetic_20_Concept.Multiply_1451;
        this.Divide_1454 = Power_126_Type.ScalarArithmetic_20_Concept.Divide_1454;
        this.Modulo_1457 = Power_126_Type.ScalarArithmetic_20_Concept.Modulo_1457;
        this.Equals_1425 = Power_126_Type.Equatable_18_Concept.Equals_1425;
        this.Compare_1422 = Power_126_Type.Comparable_17_Concept.Compare_1422;
        this.Magnitude_1419 = Power_126_Type.Magnitude_16_Concept.Magnitude_1419;
        this.Value_1417 = Power_126_Type.Measure_14_Concept.Value_1417;
    }
    // field accessors
    static Watts_1074 = function(self) { return self.Watts_1074; }
    // implemented concepts 
    static Value_22_Concept = new Value_22_Concept(Power_126_Type);
    static ScalarArithmetic_20_Concept = new ScalarArithmetic_20_Concept(Power_126_Type);
    static Equatable_18_Concept = new Equatable_18_Concept(Power_126_Type);
    static Comparable_17_Concept = new Comparable_17_Concept(Power_126_Type);
    static Magnitude_16_Concept = new Magnitude_16_Concept(Power_126_Type);
    static Measure_14_Concept = new Measure_14_Concept(Power_126_Type);
    static Implements = [Value_22_Concept,ScalarArithmetic_20_Concept,Equatable_18_Concept,Comparable_17_Concept,Magnitude_16_Concept,Measure_14_Concept];
}
class Density_127_Type
{
    constructor(KilogramsPerMeterCubed_1078)
    {
        // field initialization 
        this.KilogramsPerMeterCubed_1078 = KilogramsPerMeterCubed_1078;
        this.Type_1466 = Density_127_Type.Value_22_Concept.Type_1466;
        this.FieldTypes_1467 = Density_127_Type.Value_22_Concept.FieldTypes_1467;
        this.FieldNames_1468 = Density_127_Type.Value_22_Concept.FieldNames_1468;
        this.FieldValues_1470 = Density_127_Type.Value_22_Concept.FieldValues_1470;
        this.Zero_1471 = Density_127_Type.Value_22_Concept.Zero_1471;
        this.One_1472 = Density_127_Type.Value_22_Concept.One_1472;
        this.Default_1473 = Density_127_Type.Value_22_Concept.Default_1473;
        this.MinValue_1474 = Density_127_Type.Value_22_Concept.MinValue_1474;
        this.MaxValue_1475 = Density_127_Type.Value_22_Concept.MaxValue_1475;
        this.ToString_1477 = Density_127_Type.Value_22_Concept.ToString_1477;
        this.Add_1445 = Density_127_Type.ScalarArithmetic_20_Concept.Add_1445;
        this.Subtract_1448 = Density_127_Type.ScalarArithmetic_20_Concept.Subtract_1448;
        this.Multiply_1451 = Density_127_Type.ScalarArithmetic_20_Concept.Multiply_1451;
        this.Divide_1454 = Density_127_Type.ScalarArithmetic_20_Concept.Divide_1454;
        this.Modulo_1457 = Density_127_Type.ScalarArithmetic_20_Concept.Modulo_1457;
        this.Equals_1425 = Density_127_Type.Equatable_18_Concept.Equals_1425;
        this.Compare_1422 = Density_127_Type.Comparable_17_Concept.Compare_1422;
        this.Magnitude_1419 = Density_127_Type.Magnitude_16_Concept.Magnitude_1419;
        this.Value_1417 = Density_127_Type.Measure_14_Concept.Value_1417;
    }
    // field accessors
    static KilogramsPerMeterCubed_1078 = function(self) { return self.KilogramsPerMeterCubed_1078; }
    // implemented concepts 
    static Value_22_Concept = new Value_22_Concept(Density_127_Type);
    static ScalarArithmetic_20_Concept = new ScalarArithmetic_20_Concept(Density_127_Type);
    static Equatable_18_Concept = new Equatable_18_Concept(Density_127_Type);
    static Comparable_17_Concept = new Comparable_17_Concept(Density_127_Type);
    static Magnitude_16_Concept = new Magnitude_16_Concept(Density_127_Type);
    static Measure_14_Concept = new Measure_14_Concept(Density_127_Type);
    static Implements = [Value_22_Concept,ScalarArithmetic_20_Concept,Equatable_18_Concept,Comparable_17_Concept,Magnitude_16_Concept,Measure_14_Concept];
}
class NormalDistribution_128_Type
{
    constructor(Mean_1082, StandardDeviation_1086)
    {
        // field initialization 
        this.Mean_1082 = Mean_1082;
        this.StandardDeviation_1086 = StandardDeviation_1086;
        this.Type_1466 = NormalDistribution_128_Type.Value_22_Concept.Type_1466;
        this.FieldTypes_1467 = NormalDistribution_128_Type.Value_22_Concept.FieldTypes_1467;
        this.FieldNames_1468 = NormalDistribution_128_Type.Value_22_Concept.FieldNames_1468;
        this.FieldValues_1470 = NormalDistribution_128_Type.Value_22_Concept.FieldValues_1470;
        this.Zero_1471 = NormalDistribution_128_Type.Value_22_Concept.Zero_1471;
        this.One_1472 = NormalDistribution_128_Type.Value_22_Concept.One_1472;
        this.Default_1473 = NormalDistribution_128_Type.Value_22_Concept.Default_1473;
        this.MinValue_1474 = NormalDistribution_128_Type.Value_22_Concept.MinValue_1474;
        this.MaxValue_1475 = NormalDistribution_128_Type.Value_22_Concept.MaxValue_1475;
        this.ToString_1477 = NormalDistribution_128_Type.Value_22_Concept.ToString_1477;
    }
    // field accessors
    static Mean_1082 = function(self) { return self.Mean_1082; }
    static StandardDeviation_1086 = function(self) { return self.StandardDeviation_1086; }
    // implemented concepts 
    static Value_22_Concept = new Value_22_Concept(NormalDistribution_128_Type);
    static Implements = [Value_22_Concept];
}
class PoissonDistribution_129_Type
{
    constructor(Expected_1090, Occurrences_1094)
    {
        // field initialization 
        this.Expected_1090 = Expected_1090;
        this.Occurrences_1094 = Occurrences_1094;
        this.Type_1466 = PoissonDistribution_129_Type.Value_22_Concept.Type_1466;
        this.FieldTypes_1467 = PoissonDistribution_129_Type.Value_22_Concept.FieldTypes_1467;
        this.FieldNames_1468 = PoissonDistribution_129_Type.Value_22_Concept.FieldNames_1468;
        this.FieldValues_1470 = PoissonDistribution_129_Type.Value_22_Concept.FieldValues_1470;
        this.Zero_1471 = PoissonDistribution_129_Type.Value_22_Concept.Zero_1471;
        this.One_1472 = PoissonDistribution_129_Type.Value_22_Concept.One_1472;
        this.Default_1473 = PoissonDistribution_129_Type.Value_22_Concept.Default_1473;
        this.MinValue_1474 = PoissonDistribution_129_Type.Value_22_Concept.MinValue_1474;
        this.MaxValue_1475 = PoissonDistribution_129_Type.Value_22_Concept.MaxValue_1475;
        this.ToString_1477 = PoissonDistribution_129_Type.Value_22_Concept.ToString_1477;
    }
    // field accessors
    static Expected_1090 = function(self) { return self.Expected_1090; }
    static Occurrences_1094 = function(self) { return self.Occurrences_1094; }
    // implemented concepts 
    static Value_22_Concept = new Value_22_Concept(PoissonDistribution_129_Type);
    static Implements = [Value_22_Concept];
}
class BernoulliDistribution_130_Type
{
    constructor(P_1098)
    {
        // field initialization 
        this.P_1098 = P_1098;
        this.Type_1466 = BernoulliDistribution_130_Type.Value_22_Concept.Type_1466;
        this.FieldTypes_1467 = BernoulliDistribution_130_Type.Value_22_Concept.FieldTypes_1467;
        this.FieldNames_1468 = BernoulliDistribution_130_Type.Value_22_Concept.FieldNames_1468;
        this.FieldValues_1470 = BernoulliDistribution_130_Type.Value_22_Concept.FieldValues_1470;
        this.Zero_1471 = BernoulliDistribution_130_Type.Value_22_Concept.Zero_1471;
        this.One_1472 = BernoulliDistribution_130_Type.Value_22_Concept.One_1472;
        this.Default_1473 = BernoulliDistribution_130_Type.Value_22_Concept.Default_1473;
        this.MinValue_1474 = BernoulliDistribution_130_Type.Value_22_Concept.MinValue_1474;
        this.MaxValue_1475 = BernoulliDistribution_130_Type.Value_22_Concept.MaxValue_1475;
        this.ToString_1477 = BernoulliDistribution_130_Type.Value_22_Concept.ToString_1477;
    }
    // field accessors
    static P_1098 = function(self) { return self.P_1098; }
    // implemented concepts 
    static Value_22_Concept = new Value_22_Concept(BernoulliDistribution_130_Type);
    static Implements = [Value_22_Concept];
}
class Probability_131_Type
{
    constructor(Value_1102)
    {
        // field initialization 
        this.Value_1102 = Value_1102;
        this.Type_1466 = Probability_131_Type.Value_22_Concept.Type_1466;
        this.FieldTypes_1467 = Probability_131_Type.Value_22_Concept.FieldTypes_1467;
        this.FieldNames_1468 = Probability_131_Type.Value_22_Concept.FieldNames_1468;
        this.FieldValues_1470 = Probability_131_Type.Value_22_Concept.FieldValues_1470;
        this.Zero_1471 = Probability_131_Type.Value_22_Concept.Zero_1471;
        this.One_1472 = Probability_131_Type.Value_22_Concept.One_1472;
        this.Default_1473 = Probability_131_Type.Value_22_Concept.Default_1473;
        this.MinValue_1474 = Probability_131_Type.Value_22_Concept.MinValue_1474;
        this.MaxValue_1475 = Probability_131_Type.Value_22_Concept.MaxValue_1475;
        this.ToString_1477 = Probability_131_Type.Value_22_Concept.ToString_1477;
        this.Add_1428 = Probability_131_Type.Arithmetic_19_Concept.Add_1428;
        this.Negative_1430 = Probability_131_Type.Arithmetic_19_Concept.Negative_1430;
        this.Reciprocal_1432 = Probability_131_Type.Arithmetic_19_Concept.Reciprocal_1432;
        this.Multiply_1435 = Probability_131_Type.Arithmetic_19_Concept.Multiply_1435;
        this.Divide_1438 = Probability_131_Type.Arithmetic_19_Concept.Divide_1438;
        this.Modulo_1441 = Probability_131_Type.Arithmetic_19_Concept.Modulo_1441;
        this.Equals_1425 = Probability_131_Type.Equatable_18_Concept.Equals_1425;
        this.Compare_1422 = Probability_131_Type.Comparable_17_Concept.Compare_1422;
        this.Magnitude_1419 = Probability_131_Type.Magnitude_16_Concept.Magnitude_1419;
    }
    // field accessors
    static Value_1102 = function(self) { return self.Value_1102; }
    // implemented concepts 
    static Value_22_Concept = new Value_22_Concept(Probability_131_Type);
    static Arithmetic_19_Concept = new Arithmetic_19_Concept(Probability_131_Type);
    static Equatable_18_Concept = new Equatable_18_Concept(Probability_131_Type);
    static Comparable_17_Concept = new Comparable_17_Concept(Probability_131_Type);
    static Magnitude_16_Concept = new Magnitude_16_Concept(Probability_131_Type);
    static Numerical_15_Concept = new Numerical_15_Concept(Probability_131_Type);
    static Implements = [Value_22_Concept,Arithmetic_19_Concept,Equatable_18_Concept,Comparable_17_Concept,Magnitude_16_Concept,Numerical_15_Concept];
}
class BinomialDistribution_132_Type
{
    constructor(Trials_1106, P_1110)
    {
        // field initialization 
        this.Trials_1106 = Trials_1106;
        this.P_1110 = P_1110;
        this.Type_1466 = BinomialDistribution_132_Type.Value_22_Concept.Type_1466;
        this.FieldTypes_1467 = BinomialDistribution_132_Type.Value_22_Concept.FieldTypes_1467;
        this.FieldNames_1468 = BinomialDistribution_132_Type.Value_22_Concept.FieldNames_1468;
        this.FieldValues_1470 = BinomialDistribution_132_Type.Value_22_Concept.FieldValues_1470;
        this.Zero_1471 = BinomialDistribution_132_Type.Value_22_Concept.Zero_1471;
        this.One_1472 = BinomialDistribution_132_Type.Value_22_Concept.One_1472;
        this.Default_1473 = BinomialDistribution_132_Type.Value_22_Concept.Default_1473;
        this.MinValue_1474 = BinomialDistribution_132_Type.Value_22_Concept.MinValue_1474;
        this.MaxValue_1475 = BinomialDistribution_132_Type.Value_22_Concept.MaxValue_1475;
        this.ToString_1477 = BinomialDistribution_132_Type.Value_22_Concept.ToString_1477;
    }
    // field accessors
    static Trials_1106 = function(self) { return self.Trials_1106; }
    static P_1110 = function(self) { return self.P_1110; }
    // implemented concepts 
    static Value_22_Concept = new Value_22_Concept(BinomialDistribution_132_Type);
    static Implements = [Value_22_Concept];
}
