   M S B A M L   `   `   `  ����  SkeletalViewer    SkeletalViewer.MainWindow   5   	   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   H   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     6�   -   5          � Title$  
MainWindow��5   	    � Icon$ SkeletalViewer.ico��6    � 
ResizeMode$ NoResize=�66    � SizeToContent$ WidthAndHeight=�6L    � WindowStartupLocation$ CenterScreen=�6k   .��5      �� .+�5   
   ��  �� Orientation$ 
Horizontal=�6   .+�5      �� $	��334��6   .+�5	      �� $��18(�6   |�.  $��0q�6:   $��Depth Stream��6N   $��Center=�6b   �� 5
      -   5        $��depth��5
   1   $	��400��6   $	��300��6$   $��
10 0 10 10q�6>   5      �� 5      .+�5      �� -   5        $��18(�5      |�.  $��0q�6:   $4��.Skeleton (rendered if full body fits in frame)��6N   $��Center=�6�   6�   �� 5      -   5        $��skeleton��5   2   $	��400��6   $	��300��6%   $��
10 0 10 10q�6B   
���   �}�. 6i   5      �� 5      .+�5      �� -   5        $��VideoCaption��5   �   $��18(�6   |�.  $��0q�6:   $��Color Video Stream��6N   $��Center=�6h   6�   �� 5      -   5        $��video��5   1   $	��400��6   $	��300��6$   $��
10 0 10 10q�6>   5      5      �� 5   
   -   5        $��0q�5      |�.  $��18(�68   $����    Body                                                                                Upper Limb                                                                         Lower Limb��6F   $��59��6  $	��960��6  $��Bottom=�6?  �� 5   
   $ 
Horizontal=�6   $	��247��6.   .+�5      (� $	��404��6   $��10q�6"    (� Stretch$
 None=�6.   $��Stretch=�6=   $	��232��6Y   .��5      �� -   5        $��middle��5   (   $��15(�6   $��10q�66   |�.  $��0q�6T   $�� ��6h    �� TextAlignment$
 Left=�6q   $��Center=�6�   $��Center=�6�   $	��377��6�   $	��220��6�   5      (� 5      $	��404��6   $��10q�6"   $
 None=�6.   $��Stretch=�6=   $	��227��6Y   .��5      �� -	   5        $��upper��5   (   $��15(�6   $��10q�65   |�.  $��0q�6S   $�� ��6g   $
 Left=�6p   $��Center=�6�   $��Center=�6�   $	��377��6�   $	��228��6�   5      (� 5      $	��227��6   $
 None=�6#   $	��404��62   .��5      �� -
   5        $��lower��5   (   $��15(�6   $��10q�65   |�.  $��0q�6S   $�� ��6g   $
 Left=�6p   $��Center=�6�   $��Center=�6�   $	��377��6�   $	��220��6�   5      5       5!      5"      