   M S B A M L   `   `   `  ����L  HTelegramDeliverer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullA%clr-namespace:TelegramDeliverer.PagesTelegramDeliverer.Pages  5      &     TelegramDeliverer.Pages.MainPage   5      W SPresentationCore, Version=5.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=5.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=5.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=5.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5      R NSystem.Xaml, Version=5.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      7local%clr-namespace:TelegramDeliverer.Pages     5      b ^MaterialDesignThemes.Wpf, Version=4.1.0.2556, Culture=neutral, PublicKeyToken=df2a72020bd7962aFmaterialDesign1http://materialdesigninxaml.net/winfx/xaml/themes  5        N� Title$  MainPage��5	      $	��695��6   $	��998��6%      MaterialDesignBody#���   5
      $��Regular"�5      $��13(�5      &   System.Windows.Media.TextOptions   TextFormattingMode$ Ideal=�5         TextRenderingMode$
 Auto=�5       N� 
Background# � ��5        MaterialDesignFont N� 
FontFamily# �  5       N� 
Foreground �	#DD000000 � 	Resources 5       �� MergedDictionaries �� Source�� 5   
    5      �� 5      $f `pack://application:,,,/MaterialDesignThemes.Wpf;component/Themes/MaterialDesignTheme.Button.xaml?�6%   %    5      .��5      �� ���	#FF1D98FF.+�5   
   �� -   5        $��Text_Box��5   L     MaterialDesignTextBox#��[ 5      $��79,46,79,315q�6A   $��2��6W   
�������	 �� TextAlignment$	 Center=�5      $.��(הכנס טקסט כאן Enter Text Here��6)   $��24(�6N   
 �� AcceptsReturn$

 True��6\   $��Arial)�5       J� Opacity$	 0.9Q�6%   $��3,3,3,3q�63   $	��260��5   5    �� Left$ 56��6B   
 �� Top$	 -19��5      $	��745��6#   6/   � 5   
   $	��161��6   $	 745��6   $	 292��6-   $	��214��6>    � RowDefinitions 5       �� 5!      $
��228* �6    �� 5"      $��* �6    .+�5$      �� $��תוים��6   $��28(�6$    J� RenderTransformOrigin$ 2.602,0.378&�62   $��0,10,91,164q�6V   �� 5%      -   5        $��CharCounter��5%      $��0��6,    �� TextAlignment$ Center=�5&      $��28(�6+   $	 0.7Q�69   $��79,10,0,164q�6G   5'      � 5(   
   $	��161��6   $	 135��6   $	 292��6.   $	��214��6?    5)      �� 5*      $
��228* �6    �� 5+      $��* �6    .+�5-      �� $��28(�6   $ 2.602,0.378&�6&   $��מקטעים��6J   $��0,10,20,159q�6X   �� 5.      -   5         ��Name$ TextBlockCounter��5.      $��0��63   $ Center=�5/      $��28(�6(   $	 0.7Q�66   $��86,10,-22,170q�6D   50      �� 51   
   -   5         $  MaterialDesignRaisedAccentButton#��[ 51      ���	#FF045BD3$��48��6c   $��נקה��6o   $	 135��6}   $	 374��6�   $	��146��6�   $��20(�6�   $
��Bold"�6�   ���	#FFCEEEEE���	#DD0000006�   � 52   
   -   5        $��SaveAsPopUp��52   m   $y�	Collapsed=�6   ���	#FF54A9F3$	 212��6=   $	 460��6O   $	��212��6`   .+�53      �� -   5        $��SaveAsTextbox��55   0   #��[ 53      $��61,32,171,102q�6E   
�������$	 Center=�54      $��24(�6)   $

 True��67   $��Comic Sans MS)�6M   $	 0.9Q�6h   $��3,3,3,3q�55      $��78��6E   $ 69��6Q   $ 28��6b   �� 56      -   5        #��[ 56      $��48��6O   $��שמור בשם��6[   $	 291��6n   $	 193��6�   $��25(�6�   $
��Bold"�6�   ���	#FFCEEEEE���	#DD000000$��264,135,171,29q�6�   �� 57      $��30(�6   $ 2.602,0.378&�6&   $��47��6J   $	 603��6V   $ 93��6h   $	��161��6x   $��429,48,0,118q�6�   �� 6�    �� FlowDirection$ RightToLeft=�6�    � Language$ he-il�6�   $�שמור בשם:��6�   6�   �� 58      -   5        #��[ 58      $��48��6O   $��
ביטול��6[   $��25(�6k   $
��Bold"�6y   ���	#FFCEEEEE���	#DD000000$��61,135,374,29q�6�   59      �� 5:   
   -	   5        $��SaveBtn��5:   K   #��[ 6   $��48��6Z   $��שמור��6f   $	 592��6u   $	 530��6�   $	��234��6�   $��25(�6�   $
��Bold"�6�   ���	#FFCEEEEE���	#DD000000���	#FF045BD36	  �� 5;   
   -
   5        $��SendBtn��5;   c   #��[ 6   ���	#FF045BD3$��48��6r   $��
שליחה��6~   $	 208��6�   $	 530��6�   $	��234��6�   $��25(�6�   $
��Bold"�6�   ���	#FFCEEEEE���	#DD000000 �� 	IsDefault$
 True��6   �� 5<   
   -   5        #��[ 5<      ���	#FF045BD3$��48��6c   $$��הוסף תמונה/סרטון��6o   $	 592��6�   $	 374��6�   $	��288��6�   $��20(�6�   $
��Bold"�6�   ���	#FFCEEEEE���	#DD0000006  �� 5=   
   -   5        $��
Media_Path��5?      $ Center=�5=      $��17(�6+   $��Comic Sans MS)�5>      $	 0.9Q�6(   $��98��66   $	 369��6B   $	 427��5?       $	��619��61   �� 5@   
   -   5        $��DeleteMediaBtn��5@   @   ���	#FF045BD3$	��1.7q�6*    � ToolTip$
 Icon��6V   $��2��6e   $ 28.175,20.075&�6u   $	 508��6�   $	 374��6�   $��48��6�   $��62��6�   ��5A      �� 5B       �� 
TargetType" ��6   .�5C      ��  �� Property � �� �� Value 60   0  *MaterialDesignThemes.Wpf.PackIconExtension     Kind$ Delete=�5D      5F      5G      5H      