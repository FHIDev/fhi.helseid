infile = open("FromVolven.txt")
outfile = open("..\Fhi.HelseId\Web\Hpr\Core\Kodekonstanter.g.cs","w")
outfile.write("namespace Fhi.HelseId.Web.Hpr.Core\n");
outfile.write("{\n")
outfile.write("    public static partial class Kodekonstanter\n")
outfile.write("    {\n")
for line in infile:
    parts = line.split()
    kode=parts[0]
    if len(parts)==3:
        beskrivelse=parts[1]+'_'+parts[2]
    else:
        beskrivelse=parts[1] 
    term=beskrivelse.replace("/","_")       
    outline =     '         public static OId9060 OId9060'+term+' = new OId9060("'+kode+')";\n'
    outfile.write(outline)
    outline =     '         public static OId9060 OId9060'+term+'Beskrivelse = new OId9060("'+beskrivelse+')";\n'
    outfile.write(outline)

    
outfile.write("    }\n")
outfile.write("}\n")
outfile.close()
infile.close()