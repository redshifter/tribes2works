import java.io.File;
import java.io.IOException;
import java.nio.charset.Charset;
import java.nio.file.Files;
import java.nio.file.StandardOpenOption;
import java.text.SimpleDateFormat;
import java.util.LinkedList;
import java.util.List;

/**
 * Compile it yo damn self.
 * @author Red Shifter
 *
 */
public class SkiFreeCreator {
	private static File inputFile;
	private static File outputFile;
	private static File metaFile;
	
	private static List<String> outputText = new LinkedList<>();
	private static List<Terrain> terrainList = new LinkedList<>();
	private static int ERRORLEVEL = 0;
	
	public static void main(String[] args) {
		if( args.length == 0 || args.length > 3 ) {
			System.out.println("Parameters: inFile terFile metaFile");
			System.out.println("inFile should be the input CSV. Make sure it's saved as UTF-8.");
			System.out.println("terFile (optional) should be the output CS that will have Normal and Superhard terrains (for server use)");
			System.out.println("metaFile (optional) should be the output CS that will have Reject terrains and terrain metadata (for client use)");
			System.exit(1);
		}
		inputFile = new File(args[0]);
		if( !inputFile.exists() ) {
			System.err.println("Input file doesn't exist.");
			System.exit(1);
		}
		
		if( args.length > 1 ) outputFile = new File(args[1]);
		if( args.length > 2 ) metaFile = new File(args[2]);
		
		try {
			generateTerrain(args);
			generateMeta(args);
		}
		catch( Exception e ) {
			e.printStackTrace();
			ERRORLEVEL = 1;
		}
		
		if( outputFile != null ) {
			if( ERRORLEVEL == 0 ) {
				System.out.println("Task completed successfully");
			}
			else {
				System.out.println("Task completed with errors");
			}
		}
		else if( ERRORLEVEL == 1 ) {
			System.out.println("");
			System.out.println("Script generated with errors. Please correct them.");
		}

		System.exit(ERRORLEVEL);
	}
	
	private static void generateTerrain(String[] args) throws IOException {
		outputText.clear();

		println("// SkiFree Terrain List");
		println("// Input File Date: " + new SimpleDateFormat("yyyy-MM-dd HH:mm:ss").format(inputFile.lastModified()));
		println();
		println("// A good terrain has the following qualities:");
		println("// - doesn't have a bunch of flat ground, even if it's right outside the mission bounds (high octane)"); 
		println("// - is not fucking gigantic (stripmine, a bunch of other TR2 terrains)");
		println("// - doesn't have a bunch of steep plateaus");
		println("// - is not Magnum (a map where the fastest route is to discjump off a bunch of flat ridges)");
		println("// use $TerrainTest to test a terrain locally");
		println();
		println("%i = -1; // %i++ is pre-increment for some reason; it's -1 so it can start at 0");
		println("%j = -1; // %j++ is pre-increment for some reason; it's -1 so it can start at 0");
		println("%k = -1; // %k++ is pre-increment for some reason; it's -1 so it can start at 0");
		println();
		
		List<String> fileLines = Files.readAllLines(inputFile.toPath(), Charset.forName("UTF-8"));
		
		for( String line : fileLines ) {
			String[] split = line.split(",");
			if( split.length == 0 ) continue;
			if( !split[0].endsWith(".ter") ) continue;
			
			Terrain ter = new Terrain(split);
			if( !ter.hasErrors )
				terrainList.add(ter);
			else
				ERRORLEVEL = 1;
		}
		
		// if you don't want me at my new String[][] {}, you don't deserve me at my ______________
		for( String[] output : new String[][] {
			{"ACCEPTED TERRAINS", ""},
			{"SUPERHARD (APRIL FOOLS)", "SUPERHARD"},
			{"REJECTED FOR DEADSTOPS", "DEADSTOPS"},
			{"REJECTED FOR BEING UNSKIIABLE", "UNSKIIABLE"},
			{"REJECTED FOR SOME OTHER REASON", "OTHER"},
			{"DUPLICATES", "DUPLICATE"}
		}) {
			String listName = output[0];
			String rejectReason = output[1];
			
			// count up how many there are
			int count = 0;
			for( Terrain ter : terrainList ) {
				if( rejectReason.equals(ter.rejectReason) ) {
					count++;
				}
			}
			
			println("// " + listName + " (" + count + ")");
			for( Terrain ter : terrainList ) {
				if( rejectReason.equals(ter.rejectReason) ) {
					println(ter.toString());
				}
			}
			
			println();
		}
		
		println("$SkiFreeTerrainListMAX = %i;");
		println("$SkiFreeTerrainListSuperHardMAX = %j;");
		println("$SkiFreeTerrainListRejectedMAX = %k;");

		writeTerrainFile();
	}
	
	private static void generateMeta(String[] args) throws IOException {
		outputText.clear();
		
		println("// SkiFree Terrain Metadata");
		println("// Input File Date: " + new SimpleDateFormat("yyyy-MM-dd HH:mm:ss").format(inputFile.lastModified()));
		println();
		println("// This file contains terrain metadata. It's only meant to be read from the client.");
		println();
		
		// if you don't want me at my new String[][] {}, you don't deserve me at my ______________
		for( Terrain terrain : terrainList ) {
			String var = "$SkiFreeMeta[\"" + terrain.terrainName + "\","; 
			println(var + "MapPack] = \"" + terrain.mapPack + "\";");
			println(var + "RejectReason] = \"" + terrain.rejectReason + "\";");
		}

		writeMetadataFile();
	}
	

	private static void writeTerrainFile() throws IOException {
		if( outputFile != null) {
			Files.write(outputFile.toPath(), outputText, StandardOpenOption.TRUNCATE_EXISTING);
		}
		else {
			for( String line : outputText ) {
				System.out.println(line);
			}
		}
	}
	
	private static void writeMetadataFile() throws IOException {
		if( outputFile != null) {
			Files.write(metaFile.toPath(), outputText, StandardOpenOption.TRUNCATE_EXISTING);
		}
		else {
			for( String line : outputText ) {
				System.out.println(line);
			}
		}
	}


	private static void println() {
		println("");
	}

	private static void println(String line) {
		outputText.add(line);
	}

	static class Terrain {
		String terrainName;
		String result;
		String rejectReason;
		String comment;
		boolean hasErrors = false;
		
		String mapPack;
		
		public Terrain(String[] split) {
			terrainName = split[0];
			result = split[4];
			
			// validation
			switch( result ) {
			case "Accept":
				if( "Yes".equals(split[1]) || "Yes".equals(split[2]) || "Yes".equals(split[3]) ) {
					System.err.println(terrainName + " is Accept but has a rejection reason!");
					hasErrors = true;
				}
				break;
			case "Reject":
			case "Superhard":
				if( !"Yes".equals(split[1]) && !"Yes".equals(split[2]) && !"Yes".equals(split[3]) ) {
					System.err.println(terrainName + " is Reject but has no rejection reason!");
					hasErrors = true;
				}
				break;
			case "Duplicate":
				break;
			case "":
			default:
				System.err.println(terrainName + " has unknown result " + result);
				hasErrors = true;
				break;
			}

			if( "Superhard".equals(result) ) {
				rejectReason = "SUPERHARD";
			}
			else if( "Yes".equals(split[1]) ) {
				rejectReason = "DEADSTOPS";
			}
			else if( "Yes".equals(split[2]) ) {
				rejectReason = "UNSKIIABLE";
			}
			else if( "Yes".equals(split[3]) ) {
				rejectReason = "OTHER";
			}
			else if ( "Duplicate".equals(result) ) {
				rejectReason = "DUPLICATE";
			}
			else {
				rejectReason = "";
			}

			// metadata
			mapPack = split[5];

			// comment should always be last
			if( split.length > 6 ) {
				comment = split[6];
				if( comment.startsWith("\"") ) {
					comment = comment.substring(1, comment.length() - 1);
				}
			}
			else {
				comment = "";
			}
		}
		
		@Override
		public String toString() {
			String value;
			if( "Accept".equals(result) ) {
				value = "$SkiFreeTerrainList[%i++] = \"";
			}
			else if( "SUPERHARD".equals(rejectReason) ) {
				value = "$SkiFreeTerrainListSuperHard[%j++] = \"";
			}
			else {
				value = "$SkiFreeTerrainListRejected[%k++] = \"";
			}
			
			// automatically makes string builder on real versions of java, fuck you
			return
				value
				+ terrainName
				+ "\";"
				+ (!comment.isEmpty() ? (" // " + comment) : "")
			;
		}
	}
}
