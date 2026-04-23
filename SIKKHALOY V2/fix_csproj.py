import sys, os

csproj = os.path.join(os.path.dirname(os.path.abspath(__file__)), "EDUCATION.COM.csproj")

with open(csproj, 'r', encoding='utf-8') as f:
    c = f.read()

# ?? 1. Content entries ?????????????????????????????????????????????????????????
old_c = '<Content Include="Profile\\Invoice\\Due_Invoice.aspx" />'
new_c = (
    '<Content Include="Profile\\Invoice\\Due_Invoice.aspx" />\r\n'
    '    <Content Include="Profile\\Invoice\\ShurjoPayCallback.aspx" />\r\n'
    '    <Content Include="SQL\\Create_AAP_Invoice_OnlinePayment.sql" />'
)
if old_c in c and '<Content Include="Profile\\Invoice\\ShurjoPayCallback.aspx" />' not in c:
    c = c.replace(old_c, new_c, 1)
    print("Content entries added.")
else:
    print("Content entries already present or anchor missing.")

# ?? 2. Compile entries ?????????????????????????????????????????????????????????
anchor = (
    '    <Compile Include="Profile\\Invoice\\Due_Invoice.aspx.designer.cs">\r\n'
    '      <DependentUpon>Due_Invoice.aspx</DependentUpon>\r\n'
    '    </Compile>'
)
insert = (
    '\r\n    <Compile Include="Profile\\Invoice\\ShurjoPayCallback.aspx.cs">\r\n'
    '      <DependentUpon>ShurjoPayCallback.aspx</DependentUpon>\r\n'
    '      <SubType>ASPXCodeBehind</SubType>\r\n'
    '    </Compile>\r\n'
    '    <Compile Include="Profile\\Invoice\\ShurjoPayService.cs" />'
)
if anchor in c and 'ShurjoPayCallback.aspx.cs' not in c:
    c = c.replace(anchor, anchor + insert, 1)
    print("Compile entries added.")
else:
    print("Compile entries already present or anchor missing.")

with open(csproj, 'w', encoding='utf-8') as f:
    f.write(c)
print("Done.")

