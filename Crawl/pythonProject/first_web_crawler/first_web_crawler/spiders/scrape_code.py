import scrapy
import time
from scrapy import signals
from pydispatch import dispatcher

unique_urls = set()
class QuotesSpider(scrapy.Spider):
    name = "quotes"
    start_urls = [
        "https://www.engr.uky.edu/"
    ]

    def __init__(self):
        dispatcher.connect(self.spider_closed, signals.spider_closed)

    def parse(self, response):
        time.sleep(0.1)
        for file_extension in response.css("link::attr(href)").getall():
            file_extension = file_extension[-4:]
            file_extension_finder = file_extension.find(".")
            if file_extension_finder != -1 and file_extension != ".edu" and file_extension != ".css":
                with open('file_extension_output.txt', 'a') as f:
                    f.write(file_extension + "\n")

        for next_page in response.css('a::attr(href)').getall():
            file_extension = next_page[-4:]
            file_extension_finder = file_extension.find(".")
            if file_extension_finder != -1 and file_extension != ".edu" and file_extension != ".css":
                with open('file_extension_output.txt', 'a') as f:
                    f.write(file_extension + "\n")

            if next_page is not None:
                nullifyNextPage = False
                with open('output.txt', 'a') as f:
                    f.write(next_page + "\n")

                url_filter_result = next_page.find("http")
                tel_filter_result = next_page.find("tel")
                hash_filter_result = next_page.find("#")
                mail_filter_result = next_page.find("mailto:")

                if url_filter_result != -1 or tel_filter_result != -1 or hash_filter_result != -1 or mail_filter_result != -1 or next_page == "/":
                    nullifyNextPage = True

                if nullifyNextPage is True:
                    next_page = None

                if next_page is not None:
                    with open('filteredoutput.txt', 'a') as f:
                        f.write(next_page + "\n")

            if next_page is not None or "/includes/" or "/misc/" or "/modules/" or "/profiles/" or "/scripts/" or "/themes/" or "/CHANGELOG.txt" or "/cron.php" or "/INSTALL.mysql.txt" or "/INSTALL.pgsql.txt" or "/INSTALL.sqlite.txt" or "/install.php" or "/INSTALL.txt" or "/LICENSE.txt" or "/MAINTAINERS.txt" or "/update.php" or "/UPGRADE.txt" or "/xmlrpc.php" or "/admin/" or "/comment/reply/" or "/filter/tips/" or "/node/add/" or "/search/" or "/user/register/" or "/user/password/" or "/user/login/" or "/user/logout/" or "/?q=admin/" or "/?q=comment/reply/" or "/?q=filter/tips/" or "/?q=node/add/" or "/?q=search/" or "/?q=user/password/" or "/?q=user/register/" or "/?q=user/login/" or "/?q=user/logout/":

                next_page = response.urljoin(next_page)
                unique_urls.add(next_page)
                yield scrapy.Request(next_page, callback=self.parse)

    def spider_closed(self, spider):
        for i in unique_urls:
            with open('unique_urls_output.txt', 'a') as f:
                f.write(i +"\n")
#                 Try to create some sort of unique counter
