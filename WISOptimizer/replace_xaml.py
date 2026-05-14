import os, re
for r, d, fs in os.walk('.'):
    for f in fs:
        if f.endswith('.xaml') and f != 'App.xaml' and 'Themes' not in r:
            path = os.path.join(r, f)
            try:
                with open(path, 'r', encoding='utf-8') as file:
                    content = file.read()
                content = re.sub(r'\{StaticResource\s+([^}]*(?:Brush|Color)[^}]*)\}', r'{DynamicResource \1}', content)
                with open(path, 'w', encoding='utf-8') as file:
                    file.write(content)
            except Exception as e:
                print(e)
