import web, urllib

urls = (
	'/index', 'index'
)

class index:
    def GET(self):
        x = web.input()
        return "<b>text=<b>"

    def POST(self):
        # This is the xml string passed from the client
        x = web.data()
        
		# Remove the header "data="
        cleaned = x[5:]
          
        # Write the XML string to file
        filedir = '/home/pi/Scratch/test'
        fout = open(filedir + '/test.xml', 'w')

		# Decode url included characters
        fout.write(urllib.unquote_plus(cleaned))
        fout.close()
        return x

if __name__ == "__main__":
    app = web.application(urls, globals())
    app.run()
