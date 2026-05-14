import os, re

apply_body = """{
            try
            {
                var script = WISOptimizer.Core.DeploymentScriptGenerator.GenerateMasterScript(WISOptimizer.Core.ConfigManager.CurrentSettings.Optimization);
                _ = WISOptimizer.Core.PowerShellRunner.RunCommandAsync(script, 120);
                MessageBox.Show("Optimizations applied.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }"""

def replace_method_body(content, method_name, new_body, is_public=False):
    pattern = r'((?:private|public)\s+(?:async\s+)?(?:void|System\.Collections\.Generic\.List<string>)\s+' + method_name + r'\s*\([^)]*\)\s*)\{'
    match = re.search(pattern, content)
    if not match: return content
    start_idx = match.end() - 1
    brace_count = 0
    end_idx = -1
    for i in range(start_idx, len(content)):
        if content[i] == '{': brace_count += 1
        elif content[i] == '}':
            brace_count -= 1
            if brace_count == 0:
                end_idx = i
                break
    if end_idx != -1:
        prefix = content[:match.start()]
        sig = match.group(1).replace('async ', '')
        return prefix + sig + new_body + content[end_idx+1:]
    return content

for r, d, fs in os.walk('UI/Views'):
    for f in fs:
        if f.endswith('.xaml.cs'):
            path = os.path.join(r, f)
            with open(path, 'r', encoding='utf-8') as file:
                content = file.read()
            
            content = re.sub(r',\s*ICommandExportProvider', '', content)
            content = replace_method_body(content, 'Apply_Click', apply_body)
            content = replace_method_body(content, 'GetSelectedPowerShellCommands', '{\n            return new System.Collections.Generic.List<string>();\n        }', is_public=True)
            
            with open(path, 'w', encoding='utf-8') as file:
                file.write(content)
