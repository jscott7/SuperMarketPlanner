import web, urllib

# Imports for finding latest file
import glob
import os

urls = (
	'/index', 'index'
)

class index:
    def GET(self):
        x = web.input(date="latest")
        filedir = '/home/pi/data'
        filename = filedir

        if x.date == 'latest':
           list_of_files = glob.glob( filedir + '/*' )
           latest_file = max(list_of_files, key=os.path.getctime)
           filename = latest_file 
        else:
           filename = filedir + '/' + x.date + '.xml'

        try:
            with open(filename, 'r') as myfile:
                output = myfile.read()
        except:
            output = filename + ' is missing'

        return output

    def POST(self):
       
        # This is the xml string passed from the client
        x = web.data()
        
	# Remove the header "data="
        cleaned = x[9:]
        date = x[:8]
   
        # Write the XML string to file
        filedir = '/home/pi/data'
        fout = open(filedir + '/' + date +'.xml', 'w')

	# Decode url included characters
        fout.write(urllib.unquote_plus(cleaned))
        fout.close()
        return x

if __name__ == "__main__":
    app = web.application(urls, globals())
    app.run()
